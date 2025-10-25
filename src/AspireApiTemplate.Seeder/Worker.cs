using AspireApiTemplate.Data;
using AspireApiTemplate.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;

namespace AspireApiTemplate.Seeder;

public class Worker(IServiceProvider serviceProvider,
                    IHostApplicationLifetime hostApplicationLifetime)
    : BackgroundService
{
    #region Private Fields
    private readonly IHostApplicationLifetime _hostApplicationLifetime = hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    #endregion Private Fields

    #region Protected Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var templateContext = scope.ServiceProvider.GetRequiredService<TemplateContext>();

        await EnsureDatabaseAsync(templateContext, stoppingToken);
        await RunMigrationAsync(templateContext, stoppingToken);
        await SeedDataAsync(templateContext, stoppingToken);

        _hostApplicationLifetime.StopApplication();
    }

    #endregion Protected Methods

    #region Private Methods

    private static async Task EnsureDatabaseAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Create the database if it does not exist.
            // Do this first so there is then a database to start a transaction against.
            if (!await dbCreator.ExistsAsync(cancellationToken))
            {
                await dbCreator.CreateAsync(cancellationToken);
            }
        });
    }

    private static async Task RunMigrationAsync(DbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () => await dbContext.Database.MigrateAsync(cancellationToken));
    }

    private async Task SeedDataAsync(TemplateContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            // Seed the database here
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            dbContext.Examples.RemoveRange(dbContext.Examples);

            await dbContext.Examples.AddRangeAsync(
                [
                    new Example { ExampleKey = 1 },
                    new Example { ExampleKey = 2 }
                ], cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    #endregion Private Methods
}