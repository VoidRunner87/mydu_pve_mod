using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Helpers;
using NQ;

// ReSharper disable once CheckNamespace
#pragma warning disable CA1050
public class MyDuMod : IMod
#pragma warning restore CA1050
{
    public string GetName() => "Dynamic Encounters Mod";

    public async Task Initialize(IServiceProvider serviceProvider)
    {
        Console.WriteLine($"Initializing {GetName()}");
        
        var logger = serviceProvider.CreateLogger<MyDuMod>();
        
        await Task.Yield();
        
        var assembly = Assembly.GetExecutingAssembly();
        var executingDir = Path.GetDirectoryName(assembly.Location)!;

        var dllsToLoad = new List<string>
        {
            "Npgsql.dll",
            "System.Diagnostics.DiagnosticSource.dll",
            "System.Text.Json.dll",
            "Dapper.dll",
            "FluentMigrator.dll",
            "FluentMigrator.Runner.dll",
            "FluentMigrator.Runner.Postgres.dll",
        }.Select(x => Path.Combine(executingDir, x));

        foreach (var dll in dllsToLoad)
        {
            logger.LogInformation("Loading: {Dll}", dll);
            Assembly.LoadFrom(dll);
            logger.LogInformation("Loaded: {ll}", dll);
        }
        
        Console.WriteLine("Loaded DLLs and Starting Mod Tasks");
        
        try
        {
            await Task.WhenAll(
                ModRunner.StartModTask(assembly, "Mod.DynamicEncounters.SectorLoop")
            );
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Task TriggerAction(ulong playerId, ModAction action)
    {
        return Task.CompletedTask;
    }

    public Task<ModInfo> GetModInfoFor(ulong playerId, bool isPlayerAdmin)
    {
        return Task.FromResult(
            new ModInfo
            {
                name = "Dynamic Encounters Mod",
                actions = new List<ModActionDefinition>(),
            }
        );
    }
}