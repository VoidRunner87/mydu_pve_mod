using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Common.Data;
using Mod.DynamicEncounters.Features.Commands.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Commands.Services;

public class OpenPlayerBoardCommandHandler : IOpenPlayerBoardCommandHandler
{
    private readonly ILogger<OpenPlayerBoardCommandHandler> _logger =
        ModBase.ServiceProvider.CreateLogger<OpenPlayerBoardCommandHandler>();

    private readonly IModManagerGrain _modManagerGrain = ModBase.ServiceProvider.GetOrleans().GetModManagerGrain();
    
    public async Task HandleCommand(ulong instigatorPlayerId, string command)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            { nameof(instigatorPlayerId), instigatorPlayerId },
            { nameof(command), command }
        });

        await _modManagerGrain.TriggerModAction(
            instigatorPlayerId,
            new ActionBuilder()
                .OpenPlayerBoardApp()
                .Build()
        );
    }
}