using Blog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsWithRetryAsync(this WebApplication app, int maxRetries = 10)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");
        var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migration applied successfully.");
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Database migration attempt {Attempt}/{MaxRetries} failed.", attempt, maxRetries);
                if (attempt == maxRetries)
                {
                    throw;
                }

                await Task.Delay(TimeSpan.FromSeconds(Math.Min(attempt * 2, 10)));
            }
        }
    }
}
