using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Party.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Party.Services;

public class PlayerPartyCommandHandler : IPlayerPartyCommandHandler
{
    private readonly IPartyCommandParser _parser = ModBase.ServiceProvider.GetRequiredService<IPartyCommandParser>();

    private readonly ILogger<PlayerPartyCommandHandler> _logger =
        ModBase.ServiceProvider.CreateLogger<PlayerPartyCommandHandler>();

    private readonly IPlayerPartyService _playerPartyService =
        ModBase.ServiceProvider.GetRequiredService<IPlayerPartyService>();

    private readonly IPlayerAlertService _playerAlertService =
        ModBase.ServiceProvider.GetRequiredService<IPlayerAlertService>();
    
    public async Task HandleCommand(ulong instigatorPlayerId, string command)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            { nameof(instigatorPlayerId), instigatorPlayerId },
            { nameof(command), command }
        });
        
        _logger.LogInformation("Handling Command");
        
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