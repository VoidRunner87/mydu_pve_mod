using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mod.DynamicEncounters.Api;
using Mod.DynamicEncounters.Threads.Handles;
using NQutils.Config;

namespace Mod.DynamicEncounters;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();

        var migrationVersion = Environment.GetEnvironmentVariable("MIGRATION_VERSION");
        if (!string.IsNullOrEmpty(migrationVersion))
        {
            Config.ReadYamlFileFromArgs("mod", args);
            var version = int.Parse(migrationVersion);

            await ModBase.Setup(serviceCollection);

            using var scope = ModBase.ServiceProvider.CreateScope();
            ModBase.DowngradeDatabase(scope, version);

            return;
        }

        try
        {
            Config.ReadYamlFileFromArgs("mod", args);
            await ModBase.Setup(serviceCollection);

            using var scope = ModBase.ServiceProvider.CreateScope();
            ModBase.UpdateDatabase(scope);
            
            var host = CreateHostBuilder(serviceCollection, args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<MovementPriority>();
                    services.AddHostedService<HighPriority>();
                    services.AddHostedService<MediumPriority>();
                    services.AddHostedService<ConstructHandleListQueryWorker>();
                    services.AddHostedService<SectorLoaderWorker>();
                    services.AddHostedService<SectorActivationWorker>();
                    services.AddHostedService<SectorCleanupWorker>();
                    services.AddHostedService<SectorSpawnerWorker>();
                    services.AddHostedService<ExpirationNamesWorker>();
                    services.AddHostedService<CleanupWorker>();
                    services.AddHostedService<ReconnectBotWorker>();
                    services.AddHostedService<TaskQueueWorker>();
                    services.AddHostedService<CommandHandlerWorker>();
                })
                .Build();

            await host.RunAsync();

            Console.WriteLine("Finished Main");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static IHostBuilder CreateHostBuilder(IServiceCollection services, string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup(_ => new Startup(services)); });
}