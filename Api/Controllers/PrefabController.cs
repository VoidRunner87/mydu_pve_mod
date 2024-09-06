using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

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
}