using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Services;

public class FeatureService(IServiceProvider provider) : IFeatureReaderService, IFeatureWriterService
{
    private readonly ILogger<FeatureService> _logger = provider.CreateLogger<FeatureService>();
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task<bool> GetBoolValueAsync(string name, bool @default)
    {
        var stringValue = await GetStringValueAsync(name, $"{@default}");

        if (bool.TryParse(stringValue, out var boolVal))
        {
            return boolVal;
        }

        return @default;
    }

    public async Task<bool> GetEnabledValue<T>(bool defaultValue)
    {
        var type = typeof(T);
        var name = type.Name;
        var featureName = $"{name}Enabled";

        var result = await GetBoolValueAsync(featureName, defaultValue);

        _logger.LogDebug("{Feature} is {State}", name, result ? "Enabled" : "Disabled");

        return result;
    }

    public async Task<int> GetIntValueAsync(string name, int @default)
    {
        var stringValue = await GetStringValueAsync(name, $"{@default}");

        if (int.TryParse(stringValue, out var intVal))
        {
            return intVal;
        }

        return @default;
    }

    public async Task<double> GetDoubleValueAsync(string name, double defaultValue)
    {
        var stringValue = await GetStringValueAsync(name, $"{defaultValue}");

        if (double.TryParse(stringValue, out var val))
        {
            return val;
        }

        return defaultValue;
    }

    public async Task<string> GetStringValueAsync(string name, string @default)
    {
        try
        {
            using var db = _factory.Create();
            db.Open();

            var result = await db.ExecuteScalarAsync<string>(
                """
                SELECT value FROM public.mod_features WHERE LOWER(TRIM(name)) = LOWER(TRIM(@name))
                """,
                new
                {
                    name
                });

            _logger.LogDebug("Read {Feature} as value {Value}", name, result);

            return result;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Failed to retrieve feature value for '{Name}'. Falling back to default value '{Value}'", name,
                default);

            return @default;
        }
    }

    public async Task EnableStarterContentFeaturesAsync()
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            UPDATE public.mod_features SET value = 'true'
            WHERE name IN('SectorLoopEnabled', 'TaskQueueLoopEnabled', 'ConstructBehaviorLoopEnabled')
            """
        );
    }
}