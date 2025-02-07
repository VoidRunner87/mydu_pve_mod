using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Commands.Interfaces;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Commands.Services;

public class NpcKillsCommandHandler : INpcKillsCommandHandler
{
    private readonly ILogger<NpcKillsCommandHandler> _logger =
        ModBase.ServiceProvider.CreateLogger<NpcKillsCommandHandler>();

    private readonly IEventTriggerRepository _eventTriggerRepository =
        ModBase.ServiceProvider.GetRequiredService<IEventTriggerRepository>();
    
    private readonly IPlayerAlertService _playerAlertService =
        ModBase.ServiceProvider.GetRequiredService<IPlayerAlertService>();
    
    public async Task HandleCommand(ulong instigatorPlayerId, string command)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            { nameof(instigatorPlayerId), instigatorPlayerId },
            { nameof(command), command }
        });

        var count = await _eventTriggerRepository
            .GetCountOfEventsByPlayerId(instigatorPlayerId, "player_defeated_npc");

        await _playerAlertService.SendInfoAlert(
            instigatorPlayerId,
            $"{count} NPC Kills"
        );
    }
}