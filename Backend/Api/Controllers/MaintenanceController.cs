using System;
using System.Threading.Tasks;
using BotLib.BotClient;
using BotLib.Protocols;
using BotLib.Protocols.Queuing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("maintenance")]
public class MaintenanceController : Controller
{
    [HttpDelete]
    [Route("bugged-wrecks")]
    public async Task<IActionResult> CleanupBuggedWrecks()
    {
        var provider = ModBase.ServiceProvider;
        var handleRepository = provider.GetRequiredService<IConstructHandleRepository>();
        var constructService = provider.GetRequiredService<IConstructService>();

        var items = await handleRepository.FindAllBuggedPoiConstructsAsync();

        foreach (var constructId in items)
        {
            await constructService.SoftDeleteAsync(constructId);
        }
        
        return Ok(items);
    }

    [HttpPost]
    [Route("grpc/reconnect")]
    public async Task<IActionResult> ReconnectGrpc()
    {
        var provider = ModBase.ServiceProvider;
        var clientFactory = provider.GetRequiredService<IDuClientFactory>();
        ClientExtensions.UseFactory(clientFactory);

        var pi = LoginInformations.BotLogin(
            Environment.GetEnvironmentVariable("BOT_PREFIX")!,
            Environment.GetEnvironmentVariable("BOT_LOGIN")!,
            Environment.GetEnvironmentVariable("BOT_PASSWORD")!
        );

        await clientFactory.Connect(pi, allowExisting: true);
        
        return Ok();
    }
}