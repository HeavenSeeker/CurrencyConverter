
using CurrencyConverter.Services;
using CurrencyConverter.Services.Providers;
using CurrencyConverter.WebAPI.Data;
using CurrencyConverter.WebAPI.Extensions;
using CurrencyConverter.WebAPI.Services;
using CurrencyConverter.WebAPI.Swagger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Serilog;
using Serilog.Extensions.Hosting;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;

namespace CurrencyConverter.WebAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //config serilog
            builder.Host.UseSerilog((context, loggerCfg) => loggerCfg.ReadFrom.Configuration(context.Configuration));

            //configure httpClient and resilience
            IHttpClientBuilder httpClientBuilder = builder.Services.AddHttpClient<IExchangeRateProvider, FrankfurterApiClient>((HttpClient client) =>
            {
                client.BaseAddress = new("https://api.frankfurter.dev/");
            });

            httpClientBuilder.AddResilienceHandler("CustomPipeline", static pipelineBuilder =>
            {
                //add rate limiter for throttling
                pipelineBuilder.AddRateLimiter(new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions
                {
                    Window = TimeSpan.FromSeconds(30),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 1000,
                    PermitLimit = 1000
                }));

                pipelineBuilder.AddRetry(new HttpRetryStrategyOptions
                {
                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 5,
                    UseJitter = true
                });

                pipelineBuilder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    // Customize and configure the circuit breaker logic.
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    FailureRatio = 0.2,
                    MinimumThroughput = 100,
                    ShouldHandle = static args =>
                    {
                        return ValueTask.FromResult(args is
                        {
                            Outcome.Result.StatusCode:
                                HttpStatusCode.RequestTimeout or
                                    HttpStatusCode.TooManyRequests
                        });
                    }
                });

                pipelineBuilder.AddTimeout(TimeSpan.FromSeconds(5));
            }); ;

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add services to the container.
            builder.Services.AddScoped<ICurrencyConverterService, CurrencyConverterService>();
            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            builder.Services.AddResponseCaching();

            //configure EF and dbcontext
            var conString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentException("Cannot find DefaultConnection connection string.");
            builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseNpgsql(conString));

            // For Identity
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(o =>
            {
                //simplified for sake of this example!
                o.Password.RequiredUniqueChars = 0;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequireDigit = false;
                o.Password.RequiredLength = 4;
                o.Password.RequireUppercase = false;
                o.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

            // Authentication: configure jwt
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:secret"]))
                };
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.EnableTryItOutByDefault();
                });
                app.ApplyMigrations();
                await DbSeeder.SeedData(app);
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseResponseCaching();

            app.UseSerilogRequestLogging(o =>
            {
                o.EnrichDiagnosticContext = async (context, httpContext) =>
                {
                    context.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
                    var clientId = httpContext.User?.FindFirst("cid")?.Value;
                    context.Set("ClientId", clientId);
                };
            });

            app.MapControllers();

            app.Run();
        }
    }
}
