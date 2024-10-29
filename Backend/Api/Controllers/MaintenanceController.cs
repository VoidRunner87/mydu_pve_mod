using System.Threading.Tasks;
using BotLib.BotClient;
using BotLib.Protocols;
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
    public IActionResult ReconnectGrpc()
    {
        var provider = ModBase.ServiceProvider;
        ClientExtensions.UseFactory(provider.GetRequiredService<IDuClientFactory>());

        return Ok();
    }
}