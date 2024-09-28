using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("maintenance/bugged-wrecks")]
public class MaintenanceController : Controller
{
    [HttpDelete]
    [Route("")]
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
}