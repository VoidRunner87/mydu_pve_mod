using System;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("script")]
public class ScriptController(IServiceProvider provider) : Controller
{
    private readonly IScriptActionItemRepository _repository
        = provider.GetRequiredService<IScriptActionItemRepository>();

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _repository.GetAllAsync());
    }

    [HttpGet]
    [Route("{name}")]
    public async Task<IActionResult> Get(string name)
    {
        return Ok(await _repository.FindAsync(name));
    }
    
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> Update([FromBody] ScriptActionItem item)
    {
        await _repository.UpdateAsync(item);
        
        return Ok();
    }

    [HttpPut]
    [Route("")]
    public async Task<IActionResult> Create([FromBody] ScriptActionItem actionItem)
    {
        var validator = provider.GetRequiredService<IValidator<ScriptActionItem>>();

        var validationResult = await validator.ValidateAsync(actionItem);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult);
        }

        await _repository.AddAsync(actionItem);

        return Created();
    }

    [HttpDelete]
    [Route("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _repository.DeleteAsync(id);

        return Ok();
    }

    [HttpGet]
    [Route("action")]
    public IActionResult GetAllActions()
    {
        return Json(provider.GetRequiredService<IScriptActionFactory>().GetAllActions());
    }
}