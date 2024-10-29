using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Party.Interfaces;

namespace Mod.DynamicEncounters.Features.Party.Services;

public class PlayerPartyCommandHandler : IPlayerPartyCommandHandler
{
    private readonly IPartyCommandParser _parser = ModBase.ServiceProvider.GetRequiredService<IPartyCommandParser>();

    private readonly IPlayerPartyService _playerPartyService =
        ModBase.ServiceProvider.GetRequiredService<IPlayerPartyService>();

    private readonly IPlayerAlertService _playerAlertService =
        ModBase.ServiceProvider.GetRequiredService<IPlayerAlertService>();
    
    public async Task HandleCommand(ulong instigatorPlayerId, string command)
    {
        var outcome = _parser.Parse(instigatorPlayerId, command);

        var actionOutcome = await outcome.Action(_playerPartyService);

        if (actionOutcome.Success)
        {
            await _playerAlertService.SendInfoAlert(instigatorPlayerId, actionOutcome.Message);
        }
        else
        {
            await _playerAlertService.SendErrorAlert(instigatorPlayerId, actionOutcome.Message);
        }
    }
}