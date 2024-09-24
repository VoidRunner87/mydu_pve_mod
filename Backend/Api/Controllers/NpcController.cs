using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Api.Controllers.Validators;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("npc")]
public class NpcController(IServiceProvider provider) : Controller
{
    private readonly IPrefabItemRepository _repository =
        provider.GetRequiredService<IPrefabItemRepository>();

    private readonly AddNpcRequestValidator _validator = new();

    [SwaggerOperation("Adds an NPC prefab and script to the system")]
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Add([FromBody] AddNpcRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult);
        }

        request.Sanitize();

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

        var onLoadActions = new List<ScriptActionItem>
        {
            new()
            {
                Type = "for-each-handle-with-tag",
                Tags = ["pod"],
                Actions =
                [
                    new ScriptActionItem()
                    {
                        Type = "delete"
                    }
                ]
            },
            new()
            {
                Type = "expire-sector"
            }
        };
        
        onLoadActions.AddRange(lootActions);
        
        var scriptGuid = Guid.NewGuid();

        var script = new ScriptActionItem
        {
            Id = scriptGuid,
            Name = $"spawn-{request.Name}",
            Type = "spawn",
            Prefab = request.Name,
            Events =
            {
                OnLoad = onLoadActions
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

    [SwaggerSchema(Required = [nameof(Name), nameof(Folder), nameof(ConstructName), nameof(BlueprintPath)])]
    public class AddNpcRequest
    {
        [SwaggerSchema("Identifier of the NPC")]
        public string Name { get; set; }
        [SwaggerSchema("Folder where the blueprint file is")]
        public string Folder { get; set; } = "pve";
        [SwaggerSchema("Name of the construct")]
        public string ConstructName { get; set; }
        [SwaggerSchema("Name of the blueprint json file")]
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
        
        public void Sanitize()
        {
            Name = NameSanitationHelper.SanitizeName(Name);
        }
    }
}