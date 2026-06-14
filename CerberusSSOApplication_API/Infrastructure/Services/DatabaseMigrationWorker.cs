using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

internal class DatabaseMigrationWorker : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseMigrationWorker> _logger;

    public DatabaseMigrationWorker(IServiceProvider serviceProvider, ILogger<DatabaseMigrationWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<WriteDbContext>();

        try
        {
            _logger.LogInformation("Checking for pending PostgreSQL migrations...");
            if ((await context.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
            {
                await context.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("PostgreSQL migrations applied successfully.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while migrating the PostgreSQL database.");
            throw; 
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}