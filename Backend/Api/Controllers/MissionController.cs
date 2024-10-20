using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("mission")]
public class MissionController : Controller
{
    private readonly IServiceProvider _provider = ModBase.ServiceProvider;

    [Route("")]
    [HttpPut]
    public async Task<IActionResult> Create([FromBody] CreatePackageMissionRequest request)
    {
        /*
        SELECT * FROM public.data_item
        SELECT * FROM public.package_content
        SELECT * FROM public.formal_mission;

        -- data_item record
        -- package_content record
        -- source type 1 id is a container id
        -- source type 2 id is a market id
         */

        var orleans = _provider.GetOrleans();
        var missionGrain = orleans.GetPlayerFormalMissionGrain(request.PlayerId);
        
        var result = await missionGrain.Create(new FormalMissionCreation
        {
            collateral = 1000,
            reward = 2000,
            description = "Test",
            title = "A Mission Test",
            durationHours = 100,
            isNQMission = true,
            packageId = 798990140,
            destination = new FormalMissionTarget
            {
                type = FormalMissionTargetType.MarketContainer,
                location = new DetailedLocation
                {
                    absolute = new RelativeLocation
                    {
                        constructId = 1002378,
                        position = new Vec3
                        {
                            x = -70012788.28435567,
                            y = 70446218.88469237,
                            z = -10683435.138592057
                        },
                        rotation = Quat.Identity
                    }
                }
            },
            missionType = FormalMissionType.Hauling,
            source = new FormalMissionTarget
            {
                location = new DetailedLocation
                {
                    absolute = new RelativeLocation
                    {
                        constructId = 1002379,
                        position = new Vec3
                        {
                            x = -70012867.92742115,
                            y = 70446275.879067,
                            z = -10683446.506897686
                        },
                        rotation = Quat.Identity
                    }
                }
            }
        });

        return Ok(result.missionId);
    }

    public class CreatePackageMissionRequest
    {
        public ulong PlayerId { get; set; }
    }
}