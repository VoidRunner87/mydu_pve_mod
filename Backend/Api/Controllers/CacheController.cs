using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Common.Services;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("cache")]
public class CacheController : Controller
{
    [SwaggerOperation("Clears the Prefab and Script Cache and reloads it.")]
    [HttpDelete]
    [Route("script")]
    public async Task<IActionResult> ClearCache()
    {
        var provider = ModBase.ServiceProvider;

        var sw = new Stopwatch();
        sw.Start();
        
        var scriptActionRepository = provider.GetRequiredService<IRepository<IScriptAction>>();
        var prefabRepository = provider.GetRequiredService<IRepository<IPrefab>>();
        await scriptActionRepository.Clear();
        await prefabRepository.Clear();

        return Ok($"Cache Cleared. Took: {sw.Elapsed.TotalMilliseconds}ms");
    }

    [HttpGet]
    [Route("")]
    public IActionResult Get()
    {
        return Ok(
            CacheRegistry.CacheMap
                .ToDictionary(k => k.Key, v => v.Value.GetCurrentStatistics())
        );
    }
}