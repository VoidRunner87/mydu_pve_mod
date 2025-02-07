using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Commands.Interfaces;
using Mod.DynamicEncounters.Features.Party.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class CommandHandlerWorker : BackgroundService
{
    private readonly ILogger<CommandHandlerWorker> _logger =
        ModBase.ServiceProvider.CreateLogger<CommandHandlerWorker>();

    private readonly IPendingCommandRepository _pendingCommandRepository =
        ModBase.ServiceProvider.GetRequiredService<IPendingCommandRepository>();

    private DateTime _refDate = DateTime.UtcNow;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
                var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);
            
                await Tick(cts.Token);
                await Task.Delay(TimeSpan.FromMilliseconds(300), stoppingToken);
            }
            catch (Exception e)
            {
                ModBase.ServiceProvider.CreateLogger<CommandHandlerWorker>()
                    .LogError(e, "{Type} Exception: {Message}", GetType().Name, e.Message);
            }
        }
    }

    private async Task Tick(CancellationToken stoppingToken)
    {
        var now = DateTime.UtcNow;
        var commandItems = await _pendingCommandRepository.QueryAsync(_refDate);
        _refDate = now;

        foreach (var commandItem in commandItems)
        {
            if (stoppingToken.IsCancellationRequested) return;

            using var commandScope = _logger.BeginScope(new Dictionary<string, object>
            {
                { nameof(commandItem.PlayerId), commandItem.PlayerId },
                { nameof(commandItem.Message), commandItem.Message },
            });

            try
            {
                if (commandItem.Message.StartsWith("@g", StringComparison.OrdinalIgnoreCase))
                {
                    var playerPartyCommandHandler =
                        ModBase.ServiceProvider.GetRequiredService<IPlayerPartyCommandHandler>();
                    await playerPartyCommandHandler.HandleCommand(commandItem.PlayerId, commandItem.Message)
                        .WaitAsync(stoppingToken);
                }

                if (commandItem.Message.StartsWith("@kills npc", StringComparison.OrdinalIgnoreCase))
                {
                    var npcKillsCommandHandler =
                        ModBase.ServiceProvider.GetRequiredService<INpcKillsCommandHandler>();
                    await npcKillsCommandHandler.HandleCommand(commandItem.PlayerId, commandItem.Message)
                        .WaitAsync(stoppingToken);
                }

                if (commandItem.Message.StartsWith("@wac", StringComparison.OrdinalIgnoreCase))
                {
                    var warpAnchorCommandHandler =
                        ModBase.ServiceProvider.GetRequiredService<IWarpAnchorCommandHandler>();
                    await warpAnchorCommandHandler.HandleCommand(commandItem.PlayerId, commandItem.Message)
                        .WaitAsync(stoppingToken);
                }

                if (commandItem.Message.StartsWith("@m", StringComparison.OrdinalIgnoreCase))
                {
                    var openPlayerBoardCommandHandler =
                        ModBase.ServiceProvider.GetRequiredService<IOpenPlayerBoardCommandHandler>();
                    await openPlayerBoardCommandHandler.HandleCommand(commandItem.PlayerId, commandItem.Message)
                        .WaitAsync(stoppingToken);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to handle command");
            }
        }
    }
}