using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;

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
        var factionRepository = provider.GetRequiredService<IFactionRepository>();
        var factionMap = (await factionRepository.GetAllAsync())
            .ToDictionary(
                k => k.Id,
                v => v
            );

        if (!factionMap.TryGetValue(request.FactionId, out var faction))
        {
            return BadRequest("Invalid Faction");
        }

        var quests = await _proceduralQuestGeneratorService
            .Generate(
                request.PlayerId,
                request.FactionId,
                request.TerritoryId,
                request.Seed,
                10
            );

        return Ok(
            new QuestPanelViewModel(
                faction.Id,
                faction.Name,
                quests
            )
        );
    }

    public class QuestPanelViewModel
    {
        public long FactionId { get; set; }
        public string Faction { get; set; }
        public IEnumerable<QuestViewModel> Jobs { get; set; }

        public QuestPanelViewModel(long factionId, string factionName, GenerateQuestListOutcome outcome)
        {
            FactionId = factionId;
            Faction = factionName;
            Jobs = outcome.QuestList.Select(pq => new QuestViewModel(pq));
        }
    }
    
    public class QuestViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }

        public IEnumerable<QuestTaskViewModel> Tasks { get; set; }
        public IEnumerable<string> Rewards { get; set; }

        public QuestViewModel(ProceduralQuestItem item)
        {
            Id = item.Id;
            Title = item.Title;
            Type = item.Type;
            Tasks = item.TaskItems.Select(t => new QuestTaskViewModel(t));
            Rewards = item.Properties.RewardTextList;
        }
    }

    public class QuestTaskViewModel(QuestTaskItem questTaskItem)
    {
        public string Title { get; set; } = questTaskItem.Text;
        public string Position { get; set; } = $"::pos{{0,{questTaskItem.BaseConstruct ?? 0},{questTaskItem.Position.x}, {questTaskItem.Position.y}, {questTaskItem.Position.z}}}";
    }

    public class GenerateQuestsRequest
    {
        public ulong PlayerId { get; set; }
        public long FactionId { get; set; }
        public Guid TerritoryId { get; set; }
        public int Seed { get; set; }
    }
}