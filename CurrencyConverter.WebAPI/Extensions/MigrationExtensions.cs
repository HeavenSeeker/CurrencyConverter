using CurrencyConverter.WebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.WebAPI.Extensions
{
    public static class MigrationExtensions
    {
        public static void ApplyMigrations(this IApplicationBuilder builder)
        {
            using IServiceScope scope = builder.ApplicationServices.CreateScope();

            using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        }
    }
}
