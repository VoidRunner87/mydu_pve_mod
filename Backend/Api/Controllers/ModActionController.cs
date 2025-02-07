using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NQ;
using NQ.Interfaces;
using Orleans;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("mod-action")]
// ReSharper disable once InconsistentNaming
public class ModActionController : Controller
{
    [Route("{playerId:long}")]
    [HttpPost]
    public async Task<IActionResult> SendModAction(ulong playerId, [FromBody] ModActionRequest request)
    {
        var orleans = ModBase.ServiceProvider.GetRequiredService<IClusterClient>();
        var modManagerGrain = orleans.GetModManagerGrain();

        await modManagerGrain.TriggerModAction(playerId, new ModAction
        {
            playerId = request.PlayerId ?? 0,
            constructId = request.ConstructId ?? 0,
            elementId = request.ElementId ?? 0,
            actionId = request.ActionId,
            payload = JsonConvert.SerializeObject(request.Payload),
            modName = request.ModName
        });

        return Ok();
    }

    // ReSharper disable once InconsistentNaming
    public class ModActionRequest
    {
        public ulong ActionId { get; set; }
        // ReSharper disable once InconsistentNaming
        public string ModName { get; set; } = "Mod.DynamicEncounters";
        public ulong? ConstructId { get; set; }
        public ulong? ElementId { get; set; }
        public ulong? PlayerId { get; set; }
        public object Payload { get; set; }
    }
}