using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class DatabaseMigrationExtensions
{
    public static async Task ApplyPendingMigrationsAsync<TContext>(this IServiceProvider services, IConfiguration configuration, ILogger logger, CancellationToken cancellationToken = default) where TContext : DbContext
    {
        if (!configuration.GetValue("Database:ApplyMigrations", true)) { logger.LogWarning("Automatic database migrations are disabled by configuration."); return; }
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0) { logger.LogInformation("Database for {DbContext} is up to date.", typeof(TContext).Name); return; }
        try { logger.LogInformation("Applying {MigrationCount} pending migration(s) for {DbContext}: {Migrations}", pending.Count, typeof(TContext).Name, string.Join(", ", pending)); await context.Database.MigrateAsync(cancellationToken); logger.LogInformation("Database migrations for {DbContext} completed successfully.", typeof(TContext).Name); }
        catch (Exception exception) { logger.LogCritical(exception, "Database migration failed for {DbContext}. Application startup is aborted.", typeof(TContext).Name); throw; }
    }
}
