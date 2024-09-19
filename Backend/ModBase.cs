using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Backend;
using Backend.AWS;
using Backend.Business;
using Backend.Scenegraph;
using Backend.Storage;
using Backend.Voxels;
using BotLib.BotClient;
using BotLib.Protocols;
using BotLib.Protocols.Queuing;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Stubs;
using NQ.Router;
using NQ.Visibility;
using NQutils;
using NQutils.Logging;
using NQutils.Sql;
using Orleans;

namespace Mod.DynamicEncounters;

/// Mod base class
public class ModBase
{
    public static IDuClientFactory RestDuClientFactory => ServiceProvider.GetRequiredService<IDuClientFactory>();

    /// Use this to acess registered service
    public static IServiceProvider ServiceProvider;

    /// Use this to make gameplay calls, see "Interfaces/GrainGetterExtensions.cs" for what's available
    protected static IClusterClient Orleans;

    /// Use this object for various data access/modify helper functions
    protected static IDataAccessor DataAccessor;

    /// Conveniance field for mods who need a single bot
    public static Client Bot;

    public static IUserContent UserContent;
    public static IVoxelService VoxelService;
    public static IVoxelImporter VoxelImporter;
    public static ISql Sql;
    public static IGameplayBank Bank;
    public static IRDMSStorage Rdms;
    public static IPlanetList PlanetList;
    public static IS3 S3;
    public static IScenegraph SceneG;
    public static IScriptService SpawnerScripts;

    /// Create or login a user, return bot client instance
    public static async Task<Client> CreateUser(string prefix, bool allowExisting = false, bool randomize = false)
    {
        var username = prefix;
        if (randomize)
        {
            // Do not use random utilities as they are using tests random (that is seeded), and we want to be able to start the same test multiple times
            var r = new Random(Guid.NewGuid().GetHashCode());
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            username = prefix + '-' + new string(Enumerable.Repeat(0, 127 - prefix.Length)
                .Select(_ => chars[r.Next(chars.Length)]).ToArray());
        }

        var pi = LoginInformations.BotLogin(username,
            Environment.GetEnvironmentVariable("BOT_LOGIN")!,
            Environment.GetEnvironmentVariable("BOT_PASSWORD")!
        );

        return await Client.FromFactory(RestDuClientFactory, pi, allowExising: allowExisting);
    }

    /// Setup everything, must be called once at startup
    public static async Task Setup(IServiceCollection services)
    {
        //services.RegisterCoreServices();
        var queueingUrl = Environment.GetEnvironmentVariable("QUEUEING");
        if (string.IsNullOrEmpty(queueingUrl))
            queueingUrl = "http://queueing:9630";

        Console.WriteLine($"Queuing URL: {queueingUrl}");

        services
            .AddFluentMigratorCore()
            .ConfigureRunner(c =>
            {
                // TODO read from config
                c.AddPostgres()
                    .WithGlobalConnectionString(NQutils.Config.Config.Instance.postgres.ConnectionString());
                c.ScanIn(Assembly.GetExecutingAssembly()).For.Migrations();
            })
            .AddSingleton<ISql, Sql>()
            .AddSingleton<IYamlDeserializer, YamlDeserializer>()
            .AddInitializableSingleton<IGameplayBank, GameplayBank>()
            .AddSingleton<ILocalizationManager, LocalizationManager>()
            .AddTransient<IDataAccessor, DataAccessor>()
            .AddLogging(logging => logging.Setup(logWebHostInfo: true))
            .AddOrleansClient("IntegrationTests")
            .AddHttpClient()
            .AddTransient<NQutils.Stats.IStats, NQutils.Stats.FakeIStats>()
            .AddSingleton<IQueuing>(sp =>
                new StubRealQueuing(queueingUrl, sp.GetRequiredService<IHttpClientFactory>().CreateClient())
            )
            .AddSingleton<IDuClientFactory, StubDuClientFactory>()
            .AddSingleton<IS3, FakeS3.FakeS3Singleton>()
            .AddSingleton<IItemStorageService, ItemStorageService>()
            .AddInitializableSingleton<IUserContent, UserContent>()
            .AddInitializableSingleton<IVoxelService, VoxelService>()
            .AddInitializableSingleton<IVoxelImporter, VoxelService>()
            .AddInitializableSingleton<ISql, Sql>()
            .AddInitializableSingleton<IRDMSStorage, RDMSStorage>()
            .AddInitializableSingleton<IPlanetList, PlanetListStub>()
            .AddInitializableSingleton<IScenegraph, Scenegraph>()
            .AddInitializableSingleton<Internal.InternalClient, Internal.InternalClient>()
            .RegisterGRPCClient()
            .AddInitializableSingleton<IScenegraphAPI, ScenegraphAPI>()
            // Mod Starts Here
            .RegisterModFeatures()
            ;

        var sp = services.BuildServiceProvider();
        ServiceProvider = sp;
        ClientExtensions.SetSingletons(sp);
        ClientExtensions.UseFactory(sp.GetRequiredService<IDuClientFactory>());
        Orleans = ServiceProvider.GetRequiredService<IClusterClient>(); 
        DataAccessor = ServiceProvider.GetRequiredService<IDataAccessor>();
        UserContent = ServiceProvider.GetRequiredService<IUserContent>();
        VoxelService = ServiceProvider.GetRequiredService<IVoxelService>();
        VoxelImporter = ServiceProvider.GetRequiredService<IVoxelImporter>();
        SceneG = ServiceProvider.GetRequiredService<IScenegraph>();
        Sql = ServiceProvider.GetRequiredService<ISql>();
        Bank = ServiceProvider.GetRequiredService<IGameplayBank>();
        Rdms = ServiceProvider.GetRequiredService<IRDMSStorage>();
        PlanetList = ServiceProvider.GetRequiredService<IPlanetList>();
        S3 = ServiceProvider.GetRequiredService<IS3>();
        SpawnerScripts = ServiceProvider.GetRequiredService<IScriptService>();

        Console.WriteLine("Starting Services V2");
        await ServiceProvider.StartServicesV2();
        Console.WriteLine("Services Started");
        
        Console.WriteLine("Creating BOT User");
        Bot = await RefreshClient();
        Console.WriteLine("BOT User Created");
    }

    public virtual async Task Start()
    {
        try
        {
            await Loop();
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e}");
            throw;
        }
    }
    
    public static async Task<Client> RefreshClient()
    {
        return await CreateUser(Environment.GetEnvironmentVariable("BOT_PREFIX")!, true);
    }

    public static void UpdateDatabase(IServiceScope scope)
    {
        Console.WriteLine("Executing DB Migrations");
        var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        migrationRunner.MigrateUp();
        Console.WriteLine("Migrations Executed");
    }
    
    public static void DowngradeDatabase(IServiceScope scope, int version)
    {
        Console.WriteLine("Executing DB Migrations");
        var migrationRunner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        migrationRunner.MigrateDown(version);
        Console.WriteLine("Migrations Executed");
    }

    /// Override this with main bot code
    public virtual Task Loop()
    {
        return Task.CompletedTask;
    }

    /// Conveniance helper for running code forever
    public async Task SafeLoop(Func<Task> action)
    {
        while (true)
        {
            try
            {
                await action();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception in mod action: {e}");
            }
        }
    }
}