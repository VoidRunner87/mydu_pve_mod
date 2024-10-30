using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Commands.Interfaces;
using Mod.DynamicEncounters.Features.Party.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class CommandHandlerLoop(IThreadManager threadManager, CancellationToken token)
    : ThreadHandle(ThreadId.CommandHandler, threadManager, token)
{
    private readonly ILogger<CommandHandlerLoop> _logger = ModBase.ServiceProvider.CreateLogger<CommandHandlerLoop>();
    private readonly IPendingCommandRepository _pendingCommandRepository =
        ModBase.ServiceProvider.GetRequiredService<IPendingCommandRepository>();
    private readonly IPlayerPartyCommandHandler _commandHandler =
        ModBase.ServiceProvider.GetRequiredService<IPlayerPartyCommandHandler>();

    private DateTime _refDate = DateTime.UtcNow;
    
    public override async Task Tick()
    {
        var now = DateTime.UtcNow;
        var commandItems = await _pendingCommandRepository.QueryAsync(_refDate);
        _refDate = now;

        foreach (var commandItem in commandItems)
        {
            using var commandScope = _logger.BeginScope(new Dictionary<string, object>
            {
                { nameof(commandItem.PlayerId), commandItem.PlayerId },
                { nameof(commandItem.Message), commandItem.Message },
            });
            
            try
            {
                await _commandHandler.HandleCommand(commandItem.PlayerId, commandItem.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to handle command");
            }
        }
        
        ReportHeartbeat();
        Thread.Sleep(150);
    }
}