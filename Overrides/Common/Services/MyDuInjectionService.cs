using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Overrides.Common.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NQ;
using NQutils;

namespace Mod.DynamicEncounters.Overrides.Common.Services;

public class MyDuInjectionService : IMyDuInjectionService
{
    private readonly ILogger<MyDuInjectionService> _logger =
        ModServiceProvider.GetExternal<ILoggerFactory>()
            .CreateLogger<MyDuInjectionService>();

    private readonly IPub _pub = ModServiceProvider.GetExternal<IPub>();
    
    public async Task InjectJs(ulong playerId, string code)
    {
        _logger.LogInformation("Inject JS {Length}", code.Length);

        await _pub.NotifyTopic(
            Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = code
                }
            )
        );
    }
    
    public async Task InjectCss(ulong playerId, string code)
    {
        _logger.LogInformation("Inject CSS {Length}", code.Length);

        await _pub.NotifyTopic(
            Topics.PlayerNotifications(playerId),
            new NQutils.Messages.ModTriggerHudEventRequest(
                new ModTriggerHudEvent
                {
                    eventName = "modinjectjs",
                    eventPayload = $"""
                                    modApi.addInlineCss(`{code}`);
                                    """
                }
            )
        );
    }
    
    public Task UploadJson(ulong playerId, string key, JToken data)
    {
        return InjectJs(
            playerId,
            $"""
             modApi.setResourceContents(`{key}`, `application/json`, `{data}`);
             """
        );
    }

    public Task UploadJson(ulong playerId, string key, object data)
    {
        var jsonString = JsonConvert.SerializeObject(data);

        return InjectJs(
            playerId,
            $"""
             modApi.setResourceContents(`{key}`, `application/json`, `{jsonString}`);
             """
        );
    }
    
    public Task SetContext(ulong playerId, object data)
    {
        var jsonString = JsonConvert.SerializeObject(data);

        return InjectJs(
            playerId,
            $"""
             modApi.setContext({jsonString});
             """
        );
    }
}