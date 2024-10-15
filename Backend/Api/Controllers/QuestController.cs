using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Services;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("quest")]
public class QuestController(IServiceProvider provider) : Controller
{
    private readonly IProceduralQuestGeneratorService _proceduralQuestGeneratorService
        = provider.GetRequiredService<IProceduralQuestGeneratorService>();
    
    [HttpPost]
    [Route("list")]
    public async Task<IActionResult> Generate([FromBody] GenerateQuestsRequest request)
    {
        var quests = await _proceduralQuestGeneratorService
            .Generate(
                request.PlayerId,
                request.FactionId,
                request.TerritoryId,
                request.Seed,
                10
            );

        return Ok(quests);
    }

    public class GenerateQuestsRequest
    {
        public ulong PlayerId { get; set; }
        public long FactionId { get; set; }
        public Guid TerritoryId { get; set; }
        public int Seed { get; set; }
    }
}