using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mod.DynamicEncounters.Api;
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

        var apiDisabledEnvValue = Environment.GetEnvironmentVariable("API_ENABLED");
        var apiEnabled = !string.IsNullOrEmpty(apiDisabledEnvValue) && apiDisabledEnvValue == "true"
                         || apiDisabledEnvValue == "1";

        try
        {
            Config.ReadYamlFileFromArgs("mod", args);
            await ModBase.Setup(serviceCollection);

            var host = CreateHostBuilder(serviceCollection, args)
                .Build();

            using var scope = ModBase.ServiceProvider.CreateScope();
            ModBase.UpdateDatabase(scope);

            Console.WriteLine("Starting...");

            var taskList = new List<Task>
            {
                new SectorLoop().Start(),
                new ConstructBehaviorLoop().Start(),
                new HealthCheckLoop().Start(),
                new TaskQueueLoop().Start()
            };

            if (apiEnabled)
            {
                taskList.Add(host.RunAsync());
            }

            await Task.WhenAll(taskList);

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
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
}