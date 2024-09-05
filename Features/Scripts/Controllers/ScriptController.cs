using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Controllers;

[Route("script")]
public class ScriptController(IServiceProvider provider) : Controller
{
    private readonly IScriptActionItemRepository _repository
        = provider.GetRequiredService<IScriptActionItemRepository>();

    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetAll()
    {
        return Json(await _repository.GetAllAsync());
    }
}