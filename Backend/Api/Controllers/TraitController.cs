using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;
using Mod.DynamicEncounters.Features.ExtendedProperties.Repository;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("trait")]
public class TraitController : Controller
{
    private readonly ITraitRepository _traitRepository = ModBase.ServiceProvider.GetRequiredService<ITraitRepository>();
    
    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok((await _traitRepository.Get()).Map());
    }
    
    [Route("element-type/{elementTypeName}")]
    [HttpGet]
    public async Task<IActionResult> GetTraitOfElementType(string elementTypeName)
    {
        return Ok((await _traitRepository.GetElementTraits(elementTypeName)).Map());
    }

    [Route("")]
    [HttpDelete]
    public IActionResult ClearCache()
    {
        CachedTraitRepository.Clear();

        return Ok();
    }
}