using System.Threading.Tasks;
using Mod.DynamicEncounters.Overrides.Common;
using Mod.DynamicEncounters.Overrides.Common.Interfaces;
using Mod.DynamicEncounters.Overrides.Common.Services;
using NQ;

namespace Mod.DynamicEncounters.Overrides.Actions.Party;

public class RenderPartyAppAction : IModActionHandler
{
    public async Task HandleAction(ulong playerId, ModAction action)
    {
        var injection = ModServiceProvider.Get<IMyDuInjectionService>();

        await injection.InjectJs(playerId, Resources.CreatePartyRootDivJs);
        await Task.Delay(100);
        await injection.InjectCss(playerId, Resources.PartyAppCss);
        await Task.Delay(100);
        await injection.InjectJs(playerId, Resources.PartyAppJs);
    }
}