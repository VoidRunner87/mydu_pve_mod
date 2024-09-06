using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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
}