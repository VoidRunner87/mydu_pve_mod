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

        var playerService = provider.GetRequiredService<IPlayerService>();

        var map = await playerService.GetAllElementSkins(playerId);

        var filteredSkins = request.Items
            .Where(x => !map.ContainsKey(x.ElementTypeId) || !map[x.ElementTypeId].Contains(x.Skin));
        
        await playerService.GivePlayerElementSkins(
            playerId,
            filteredSkins.Select(x => new IPlayerService.ElementSkinItem
            {
                ElementTypeId = x.ElementTypeId,
                Skin = x.Skin
            })
        );

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