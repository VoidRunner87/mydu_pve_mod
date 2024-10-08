using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Threads;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("thread")]
public class ThreadController : Controller
{
    [HttpGet]
    [Route("")]
    public IActionResult GetStatus()
    {
        return Ok(
            ThreadManager.Instance
                .GetState()
        );
    }
    
    [HttpPost]
    [Route("cancel/{id}")]
    public IActionResult CancelThread(ThreadId id)
    {
        ThreadManager.Instance
            .CancelThread(id);
        
        return Ok();
    }
    
    [HttpPost]
    [Route("interrupt/{id}")]
    public IActionResult InterruptThread(ThreadId id)
    {
        ThreadManager.Instance
            .CancelThread(id);
        
        return Ok();
    }
}