using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NQ;
using NQ.Interfaces;
using Orleans;

namespace Mod.DynamicEncounters.Overrides.Actions;

public class UpgradeConstructAction(IServiceProvider provider) : IModActionHandler
{
    public async Task HandleAction(ulong playerId, ModAction action)
    {
        var orleans = provider.GetRequiredService<IClusterClient>();

        var talentGrain = orleans.GetTalentGrain(playerId);

        await talentGrain.UpgradeConstruct(action.constructId);
    }
}