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
            ThreadManager.GetInstance()
                .GetState()
        );
    }
    
    [HttpPost]
    [Route("cancel/{id}")]
    public IActionResult CancelThread(ThreadId id)
    {
        ThreadManager.GetInstance()
            .CancelThread(id);
        
        return Ok();
    }
    
    [HttpPost]
    [Route("interrupt/{id}")]
    public IActionResult InterruptThread(ThreadId id)
    {
        ThreadManager.GetInstance()
            .CancelThread(id);
        
        return Ok();
    }
    
    [HttpPost]
    [Route("block/{id}")]
    public IActionResult BlockThread(ThreadId id)
    {
        ThreadManager.GetInstance()
            .BlockThreadCreation(id);
        
        return Ok();
    }
    
    [HttpPost]
    [Route("release/{id}")]
    public IActionResult ReleaseThread(ThreadId id)
    {
        ThreadManager.GetInstance()
            .UnblockThreadCreation(id);
        
        return Ok();
    }

    [HttpPost]
    [Route("manager/stop")]
    public IActionResult StopThreadManager()
    {
        ThreadManager.GetInstance()
            .CancelAllThreads();

        return Ok();
    }
}