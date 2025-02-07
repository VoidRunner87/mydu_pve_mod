using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NQ;
using NQ.Interfaces;
using Orleans;

namespace Mod.DynamicEncounters.Overrides.Actions;

public class UpgradeConstructAction(IServiceProvider provider) : IModActionHandler
{
    public async Task HandleAction(ulong playerId, ModAction action)
    {
        var logger = provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<UpgradeConstructAction>();
        var orleans = provider.GetRequiredService<IClusterClient>();

        var talentGrain = orleans.GetTalentGrain(playerId);

        await talentGrain.UpgradeConstruct(action.constructId);
        
        logger.LogInformation("Buffed Construct {Construct}", action.constructId);
    }
}