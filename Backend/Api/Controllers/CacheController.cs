using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Features.Common.Services;
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
        await Task.Yield();
        var provider = ModBase.ServiceProvider;

        var sw = new Stopwatch();
        sw.Start();
        
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