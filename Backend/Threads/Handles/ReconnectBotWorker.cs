using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Threads.Handles;

public class ReconnectBotWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
                var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, timeoutCts.Token);

                await Tick(cts.Token);
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
            catch (Exception e)
            {
                ModBase.ServiceProvider.CreateLogger<ReconnectBotWorker>()
                    .LogError(e, "{Type} Exception: {Message}", GetType().Name, e.Message);
            }
        }
    }

    private async Task Tick(CancellationToken stoppingToken)
    {
        if (!ConstructBehaviorContextCache.IsBotDisconnected || stoppingToken.IsCancellationRequested)
        {
            return;
        }

        var logger = ModBase.ServiceProvider.CreateLogger<ReconnectBotWorker>();

        try
        {
            logger.LogWarning("Reconnecting Bot");

            await ModBase.Bot.Reconnect();
            ConstructBehaviorContextCache.RaiseBotReconnected();

            logger.LogWarning("Reconnected Bot");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to Reconnect BOT");
        }
    }
}