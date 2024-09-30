using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.TaskQueue.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("task-queue")]
public class TaskQueueController : Controller
{
    private readonly IServiceProvider _provider = ModBase.ServiceProvider;
    
    [SwaggerOperation("Enqueues a Script Execution")]
    [HttpPost]
    [Route("script")]
    public async Task<IActionResult> EnqueueScript([FromBody] EnqueueRequest request)
    {
        if (request.Script == null)
        {
            return BadRequest();
        }
        
        var taskQueueService = _provider.GetRequiredService<ITaskQueueService>();
        await taskQueueService.EnqueueScript(
            request.Script,
            request.DeliveryAt
        );

        return Created();
    }

    public class EnqueueRequest
    {
        public DateTime? DeliveryAt { get; set; }
        public ScriptActionItem? Script { get; set; }
    }
}