using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

[Route("wreck")]
public class WreckController(IServiceProvider provider) : Controller
{
    private readonly IPrefabItemRepository _repository =
        provider.GetRequiredService<IPrefabItemRepository>();

    private readonly AddWreckRequestValidator _validator = new();

    [SwaggerOperation("Adds a wreck prefab and script to the system")]
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Add([FromBody] AddWreckRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult);
        }
        
        request.Sanitize();

        var guid = Guid.NewGuid();

        var prefab = new PrefabItem
        {
            Folder = request.Folder,
            Name = request.Name,
            Id = guid,
            Path = request.BlueprintPath,
            OwnerId = 0,
            InitialBehaviors = ["wreck"],
            ServerProperties =
            {
                Header =
                {
                    PrettyName = request.ConstructName
                },
                IsDynamicWreck = true
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

    [SwaggerSchema(Required = [nameof(Name), nameof(Folder), nameof(ConstructName), nameof(BlueprintPath)])]
    public class AddWreckRequest
    {
        [SwaggerSchema("Identifier of the Wreck")]
        public string Name { get; set; }
        [SwaggerSchema("Folder where the blueprint file is")]
        public string Folder { get; set; } = "pve";
        [SwaggerSchema("Name of the construct")]
        public string ConstructName { get; set; }
        [SwaggerSchema("Name of the blueprint json file")]
        public string BlueprintPath { get; set; }

        public IEnumerable<WreckLoot> LootList { get; set; } = new List<WreckLoot>();

        public class WreckLoot
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