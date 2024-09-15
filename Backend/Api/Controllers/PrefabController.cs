using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("prefab")]
public class PrefabController(IServiceProvider provider) : Controller
{
    private readonly IPrefabItemRepository _repository =
        provider.GetRequiredService<IPrefabItemRepository>();

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _repository.GetAllAsync();

        return Json(result);
    }

    [HttpPut]
    [Route("")]
    public async Task<IActionResult> Create([FromBody] PrefabItem model)
    {
        var validator = provider.GetRequiredService<IValidator<PrefabItem>>();

        var validationResult = await validator.ValidateAsync(model);

        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult);
        }

        await _repository.AddAsync(model);

        return Created();
    }

    [HttpDelete]
    [Route("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _repository.DeleteAsync(id);

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> AddWreck([FromBody] AddWreckRequest request)
    {
        var guid = Guid.NewGuid();
        
        await _repository.AddAsync(
            new PrefabItem
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
            }
        );

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
        
        await scriptActionItemRepository.AddAsync(
            new ScriptActionItem
            {
                Id = scriptGuid,
                Name = $"spawn-{request.Name}",
                Type = "spawn",
                Prefab = request.Name,
                Events =
                {
                    OnLoad = lootActions
                }
            }
        );

        return Ok(new
        {
            PrefabId = guid,
            ScriptId = scriptGuid
        });
    }

    public class AddWreckRequest
    {
        public string Name { get; set; }
        public string Folder { get; set; } = "pve";
        public string ConstructName { get; set; }
        public string BlueprintPath { get; set; }

        public IEnumerable<WreckLoot> LootList { get; set; } = new List<WreckLoot>();
        
        public class WreckLoot
        {
            public string[] Tags = [];
            public long Budget { get; set; } = 1000;
        }
    }
}