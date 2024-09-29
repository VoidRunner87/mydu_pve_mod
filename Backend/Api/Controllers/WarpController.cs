using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Common.Interfaces;
using NQ;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("warp")]
public class WarpController : Controller
{
    [Route("anchor")]
    public async Task<IActionResult> CreateWarpAnchor([FromBody] WarpAnchorRequest request)
    {
        var provider = ModBase.ServiceProvider;
        var spawner = provider.GetRequiredService<IBlueprintSpawnerService>();

        var constructId = await spawner.SpawnAsync(
            new SpawnArgs
            {
                Folder = "pve",
                File = "Warp_Signature.json",
                Position = request.Position,
                IsUntargetable = true,
                OwnerEntityId = new EntityId{playerId = request.PlayerId},
                Name = "[!] Warp Signature"
            }
        );

        var connectionFactory = provider.GetRequiredService<IPostgresConnectionFactory>();
        using var db = connectionFactory.Create();

        // Make sure the beacon is active by setting all elements to have been created 3 days in the past *shrugs*
        await db.ExecuteAsync(
            """
            UPDATE public.element SET created_at = NOW() - INTERVAL '3 DAYS' WHERE construct_id = @constructId
            """,
            new
            {
                constructId = (long)constructId
            }
        );

        return Ok(constructId);
    }

    public class WarpAnchorRequest
    {
        public ulong PlayerId { get; set; }
        public Vec3 Position { get; set; }
    }
}