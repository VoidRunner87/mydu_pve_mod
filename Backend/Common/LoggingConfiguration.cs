using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using NQutils.Config;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Mod.DynamicEncounters.Common;

public static class LoggingConfiguration
{
    private static readonly object Lock = new();
    private static Logger? _currentLogger;
    public static readonly LoggingLevelSwitch LoggingLevelSwitch = new();

    public static void Setup(bool logEfCoreCommands = false, bool logWebHostInfo = true, string? logFileName = null)
    {
        lock (Lock)
        {
            if (_currentLogger != null)
                return;
            _currentLogger = SetupInternal(logEfCoreCommands, logWebHostInfo, logFileName);
            Log.Logger = _currentLogger;
        }
    }

    private static Logger SetupInternal(
        bool logEfCoreCommands,
        bool logWebHostInfo,
        string? logFileName)
    {
        var log = Config.Instance.log;

#if !DEBUG
        log.level = LogEventLevel.Warning;
        log.console_level = LogEventLevel.Warning;
#endif

        var interpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 2);
        interpolatedStringHandler.AppendLiteral("configuring log system from ");
        interpolatedStringHandler.AppendFormatted(Config.Instance.path);
        interpolatedStringHandler.AppendLiteral(" console: ");
        interpolatedStringHandler.AppendFormatted(log.console_level);
        Console.WriteLine(interpolatedStringHandler.ToStringAndClear());

        var loggerConfiguration =
            new LoggerConfiguration().MinimumLevel.Is(log.console_level < log.level ? log.console_level : log.level);
        loggerConfiguration.Enrich.With<ClassNameEnricher>();

        if (log.to_stdout_dev)
            loggerConfiguration.WriteTo.Console(
                log.console_level,
                "[{Timestamp:HH:mm:ss.fff} {Level:u3} {SourceContext}] {Message:lj}{Properties}{NewLine}{Exception}"
            );

        if (!log.dotnet_verbose)
        {
            loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
            if (logEfCoreCommands)
                loggerConfiguration.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command",
                    LogEventLevel.Information);
            loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Information);
            if (!logWebHostInfo)
            {
                loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Internal.WebHost",
                    LogEventLevel.Warning);
                loggerConfiguration.MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics",
                    LogEventLevel.Warning);
            }

            loggerConfiguration.MinimumLevel.Override("Orleans.Messaging.GatewayManager", LogEventLevel.Warning);
            loggerConfiguration.MinimumLevel.Override("Orleans.Runtime", LogEventLevel.Information);
            loggerConfiguration.MinimumLevel.Override("Orleans.Runtime.Management.ManagementGrain",
                LogEventLevel.Warning);
            loggerConfiguration.MinimumLevel.Override("Orleans.Runtime.SiloControl", LogEventLevel.Warning);
            loggerConfiguration.MinimumLevel.Override("Orleans.Runtime.SiloLogStatistics", LogEventLevel.Warning);
            loggerConfiguration.MinimumLevel.Override("Orleans.Runtime.GrainTypeManager", LogEventLevel.Warning);
            loggerConfiguration.MinimumLevel.Override("Orleans.Statistics.LinuxEnvironmentStatistics",
                LogEventLevel.Error);
            loggerConfiguration.MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Error);
            loggerConfiguration.MinimumLevel.Override("Orleans.Streams.PubSubRendezvousGrain", LogEventLevel.Warning);
            loggerConfiguration.MinimumLevel.Override("Orleans.Runtime.InsideRuntimeClient", LogEventLevel.Warning);
            loggerConfiguration.MinimumLevel.Override("Orleans.Runtime.Scheduler.OrleansTaskScheduler",
                LogEventLevel.Warning);
        }

        loggerConfiguration.Enrich.FromLogContext();
        var path1 = Environment.GetEnvironmentVariable("NQ_LOG_PATH") ?? log.path;
        if (logFileName == null)
            logFileName = AppDomain.CurrentDomain.FriendlyName;
        int? nullable;
        if (log.to_file_prod)
        {
            var str = Path.ChangeExtension(Path.Combine(path1, logFileName), "json");
            var writeTo = loggerConfiguration.WriteTo;

            var level = (int)log.level;
            var fileSizeLimitBytes = new long?(log.dotnet_roll_size_limit);
            nullable = (int)log.dotnet_roll_file_count;
            var flushToDiskInterval = new TimeSpan?();
            var retainedFileCountLimit = nullable;
            var retainedFileTimeLimit = new TimeSpan?();
            
            writeTo.File(str, (LogEventLevel)level,
                "[{Timestamp:HH:mm:ss.fff} {Level:u3} {SourceContext}] {Message:lj} {Properties}{NewLine}{Exception}",
                fileSizeLimitBytes: fileSizeLimitBytes, 
                shared: true, 
                flushToDiskInterval: flushToDiskInterval,
                rollOnFileSizeLimit: true, 
                retainedFileCountLimit: retainedFileCountLimit,
                retainedFileTimeLimit: retainedFileTimeLimit
            );
            
            Console.WriteLine("will log to '" + str + "'");
        }

        if (log.to_file_dev)
        {
            var path2 = logFileName + "_dev";
            var str = Path.ChangeExtension(Path.Combine(path1, path2), "log");
            var writeTo = loggerConfiguration.WriteTo;
            var level = (int)log.level;
            var fileSizeLimitBytes = new long?(log.dotnet_roll_size_limit);
            nullable = new int?((int)log.dotnet_roll_file_count);
            var flushToDiskInterval = new TimeSpan?();
            var retainedFileCountLimit = nullable;
            var retainedFileTimeLimit = new TimeSpan?();
            
            writeTo.File(str, (LogEventLevel)level,
                "[{Timestamp:HH:mm:ss.fff} {Level:u3} {SourceContext}] {Message:lj} {Properties}{NewLine}{Exception}",
                fileSizeLimitBytes: fileSizeLimitBytes,
                shared: true, 
                flushToDiskInterval: flushToDiskInterval,
                rollOnFileSizeLimit: true, 
                retainedFileCountLimit: retainedFileCountLimit,
                retainedFileTimeLimit: retainedFileTimeLimit
            );
            
            Console.WriteLine("will log to '" + str + "'");
        }
        
        return loggerConfiguration
            .MinimumLevel.ControlledBy(LoggingLevelSwitch)
            .CreateLogger();
    }

    public static void SetupPveModLog(this ILoggingBuilder loggingBuilder,
        bool logEfCoreCommands = false,
        bool logWebHostInfo = true,
        string? logFileName = null)
    {
        loggingBuilder.ClearProviders();
        Setup(logEfCoreCommands, logWebHostInfo, logFileName);
        loggingBuilder.AddSerilog(dispose: true);
    }
}