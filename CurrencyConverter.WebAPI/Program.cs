
using CurrencyConverter.Services;
using CurrencyConverter.Services.Providers;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using System.Net;

namespace CurrencyConverter.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddScoped<ICurrencyConverterService, CurrencyConverterService>();

            var httpClientBuilder = builder.Services.AddHttpClient<IExchangeRateProvider, FrankfurterApiClient>((HttpClient client) =>
            {
                client.BaseAddress = new("https://api.frankfurter.dev/");
            });

            httpClientBuilder.AddResilienceHandler("CustomPipeline", static builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 5,
                    UseJitter = true
                });

                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
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

                builder.AddTimeout(TimeSpan.FromSeconds(5));
            }); ;

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddResponseCaching();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
                    options.EnableTryItOutByDefault();
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseResponseCaching();

            app.MapControllers();

            app.Run();
        }
    }
}
