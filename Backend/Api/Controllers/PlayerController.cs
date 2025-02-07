using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("player")]
public class PlayerController : Controller
{
    [HttpPost]
    [Route("script/give-element-skin")]
    public IActionResult GetGiveElementSkinScript([FromBody] GiveElementSkinsToAllActivePlayersRequest request)
    {
        var provider = ModBase.ServiceProvider;

        var bank = provider.GetGameplayBank();

        var scriptActionItem = new ScriptActionItem
        {
            Type = GiveElementSkinToPlayer.ActionName,
            Properties =
            {
                {
                    "Skins", request.Items.Select(x => new GiveElementSkinToPlayer.ElementSkinItem
                    {
                        ElementTypeId = bank.GetDefinition(x.ElementTypeName)!.Id,
                        Skin = x.Skin
                    })
                },
            }
        };

        return Ok(scriptActionItem);
    }

    [HttpPost]
    [Route("all/give-element-skin")]
    public async Task<IActionResult> GiveElementSkin([FromBody] GiveElementSkinsToAllActivePlayersRequest request)
    {
        var provider = ModBase.ServiceProvider;

        var bank = provider.GetGameplayBank();
        var playerService = provider.GetRequiredService<IPlayerService>();
        var taskQueueService = provider.GetRequiredService<ITaskQueueService>();

        var playerIds = (await playerService.GetAllPlayersActiveOnInterval(request.Interval)).ToList();

        foreach (var playerId in playerIds)
        {
            await taskQueueService.EnqueueScript(
                new ScriptActionItem
                {
                    Type = GiveElementSkinToPlayer.ActionName,
                    Properties =
                    {
                        { "PlayerIds", new List<ulong> { playerId } },
                        {
                            "Skins", request.Items.Select(x => new GiveElementSkinToPlayer.ElementSkinItem
                            {
                                ElementTypeId = bank.GetDefinition(x.ElementTypeName)!.Id,
                                Skin = x.Skin
                            })
                        },
                    }
                },
                DateTime.UtcNow
            );
        }

        return Ok($"Enqueued {playerIds.Count} messages");
    }

    [HttpPost]
    [Route("{playerId:long}/give-element-skin")]
    public async Task<IActionResult> GiveElementSkin(ulong playerId, [FromBody] GiveElementSkinsRequest request)
    {
        var provider = ModBase.ServiceProvider;

        var bank = provider.GetGameplayBank();
        var playerService = provider.GetRequiredService<IPlayerService>();

        var map = await playerService.GetAllElementSkins(playerId);

        var filteredSkins = request.Items
            .Where(x => !map.ContainsKey(x.ElementTypeId) || !map[x.ElementTypeId].Contains(x.Skin));

        await playerService.GivePlayerElementSkins(
            playerId,
            filteredSkins.Select(x => new IPlayerService.ElementSkinItem
            {
                ElementTypeId = bank.GetDefinition(x.ElementTypeName)!.Id,
                Skin = x.Skin
            })
        );

        return Ok();
    }

    [HttpPost]
    [Route("{playerId:long}/board/{constructId:long}")]
    public async Task<IActionResult> BoardConstruct(ulong playerId, ulong constructId)
    {
        var orleans = ModBase.ServiceProvider.GetOrleans();

        var playerGrain = orleans.GetPlayerGrain(playerId);
        var constructElementsGrain = orleans.GetConstructElementsGrain(constructId);
        var seats = (await constructElementsGrain.GetElementsOfType<PVPSeatUnit>()).ToList();
        var controlUnits = (await constructElementsGrain.GetElementsOfType<ControlUnit>()).ToList();
        
        seats.AddRange(controlUnits);

        if (seats.Count == 0) NotFound();

        var elementInfo = await constructElementsGrain.GetElement(seats[0]);

        await playerGrain.TeleportPlayer(
            new RelativeLocation
            {
                rotation = Quat.Identity,
                constructId = constructId,
                position = elementInfo.position
            });

        return Ok();
    }
    public class GiveElementSkinsToAllActivePlayersRequest
    {
        public TimeSpan Interval { get; set; }
        public IEnumerable<Item> Items { get; set; }
    }

    public class GiveElementSkinsRequest
    {
        public IEnumerable<Item> Items { get; set; }
    }

    public class Item
    {
        public string ElementTypeName { get; set; }
        public ulong ElementTypeId { get; set; }
        public string Skin { get; set; }
    }
}