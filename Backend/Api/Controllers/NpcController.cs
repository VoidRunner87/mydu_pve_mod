using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Api.Controllers.Validators;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("npc")]
public class NpcController(IServiceProvider provider) : Controller
{
    private readonly IPrefabItemRepository _repository =
        provider.GetRequiredService<IPrefabItemRepository>();

    private readonly AddNpcRequestValidator _validator = new(); 

    [HttpPut]
    [Route("")]
    public async Task<IActionResult> Add([FromBody] AddNpcRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult);
        }
        
        var guid = Guid.NewGuid();

        var giveQuantaAction = new ScriptActionItem
        {
            Type = "give-quanta",
            Value = request.QuantaReward * 100,
            Message = "Kill Reward"
        };
        
        var prefab = new PrefabItem
        {
            Folder = request.Folder,
            Name = request.Name,
            Id = guid,
            Path = request.BlueprintPath,
            OwnerId = 4,
            InitialBehaviors = ["aggressive", "follow-target"],
            Events =
            {
                OnDestruction = request.QuantaReward == 0 ? [] : [giveQuantaAction]
            },
            ServerProperties =
            {
                Header =
                {
                    PrettyName = request.ConstructName
                },
                IsDynamicWreck = false
            }
        };
        
        await _repository.AddAsync(prefab);

        var scriptActionItemRepository = provider.GetRequiredService<IScriptActionItemRepository>();

        var lootActions = request.LootList
            .Select(x => new ScriptActionItem
            {
                Type = "spawn-loot",
                Tags = x.Tags.ToList(),
                Value = x.Budget
            })
            .ToList();
        
        var scriptGuid = Guid.NewGuid();

        var script = new ScriptActionItem
        {
            Id = scriptGuid,
            Name = $"spawn-{request.Name}",
            Type = "spawn",
            Prefab = request.Name,
            Events =
            {
                OnLoad = lootActions
            }
        };
        
        await scriptActionItemRepository.AddAsync(script);

        return Ok(new
        {
            PrefabId = guid,
            ScriptId = scriptGuid,
            Prefab = prefab,
            Script = script
        });
    }

    public class AddNpcRequest
    {
        public string Name { get; set; }
        public string Folder { get; set; } = "pve";
        public string ConstructName { get; set; }
        public string BlueprintPath { get; set; }
        
        public IEnumerable<string> AmmoItems { get; set; } = ["AmmoCannonSmallKineticAdvancedPrecision", "AmmoCannonSmallThermicAdvancedPrecision"];
        public IEnumerable<string> WeaponItems { get; set; } = ["WeaponCannonSmallPrecision3"];

        public long QuantaReward { get; set; } = 250000;

        public IEnumerable<NpcLoot> LootList { get; set; } = new List<NpcLoot>();
        
        public class NpcLoot
        {
            public IEnumerable<string> Tags { get; set; } = [];
            public long Budget { get; set; } = 1000;
        }
    }
}