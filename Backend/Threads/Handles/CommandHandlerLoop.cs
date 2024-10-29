using System;
using System.Threading;
using System.Threading.Tasks;
using BotLib.BotClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.Party.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQutils.Exceptions;
using Timer = System.Timers.Timer;

namespace Mod.DynamicEncounters.Threads.Handles;

public class CommandHandlerLoop(IThreadManager threadManager, CancellationToken token)
    : ThreadHandle(ThreadId.CommandHandler, threadManager, token)
{
    private readonly EventListener<MessageContent> _listener = ModBase.Bot.Events.MessageReceived.Listener();
    private readonly ILogger<CommandHandlerLoop> _logger = ModBase.ServiceProvider.CreateLogger<CommandHandlerLoop>();
    private readonly IPlayerPartyCommandHandler _commandHandler =
        ModBase.ServiceProvider.GetRequiredService<IPlayerPartyCommandHandler>();
    private readonly Timer _timer = new Timer(TimeSpan.FromSeconds(1)); 
    
    public override async Task Tick()
    {
        try
        {
            _timer.Elapsed += (_, _) => ReportHeartbeat();
            _timer.Start();
            
            var messageContent =
                await _listener.GetLastEventWait(CanHandleMessage, 60000);

            await _commandHandler.HandleCommand(messageContent.fromPlayerId, messageContent.message);

            _listener.Clear();
        }
        catch (BusinessException bex)
        {
            _logger.LogError(bex, "Business Exception");

            await ModBase.Bot.Reconnect();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failure on listening to commands");
        }
        
        _timer.Stop();
    }

    private bool CanHandleMessage(MessageContent mc)
    {
        return mc.message.StartsWith("@g");
    }
}