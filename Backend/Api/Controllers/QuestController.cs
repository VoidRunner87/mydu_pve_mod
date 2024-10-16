using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Common.Interfaces;
using Mod.DynamicEncounters.Features.Faction.Interfaces;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("quest")]
public class QuestController(IServiceProvider provider) : Controller
{
    private readonly IProceduralQuestGeneratorService _proceduralQuestGeneratorService
        = provider.GetRequiredService<IProceduralQuestGeneratorService>();

    private readonly IPlayerQuestService _playerQuestService
        = provider.GetRequiredService<IPlayerQuestService>();

    [HttpPost]
    [Route("player/accept")]
    public async Task<IActionResult> AcceptQuest([FromBody] AcceptQuestRequest request)
    {
        var quests = await _proceduralQuestGeneratorService
            .Generate(
                request.PlayerId,
                request.FactionId,
                request.TerritoryId,
                request.Seed,
                10
            );

        var questMap = quests.QuestList
            .ToDictionary(
                k => k.Id,
                v => v
            );

        if (!questMap.TryGetValue(request.QuestId, out var questItem))
        {
            return Ok(AcceptQuestResponse.Failed("No longer available. Refresh the board"));
        }

        var outcome = await _playerQuestService.AcceptQuestAsync(
            request.PlayerId,
            questMap[request.QuestId]
        );

        if (!outcome.Success)
        {
            return BadRequest(outcome);
        }
        
        return Ok(AcceptQuestResponse.Success($"Accepted '{questItem.Title}'. {outcome.Message}"));
    }

    [HttpPost]
    [Route("player/abandon")]
    public async Task<IActionResult> AcceptQuest([FromBody] AbandonQuestRequest request)
    {
        var playerQuestRepository = provider.GetRequiredService<IPlayerQuestRepository>();

        await playerQuestRepository.DeleteAsync(request.PlayerId, request.QuestId);

        return Ok();
    }

    [HttpGet]
    [Route("player/{playerId:long}")]
    public async Task<IActionResult> GetPlayerQuests(ulong playerId)
    {
        var playerQuestRepository = provider.GetRequiredService<IPlayerQuestRepository>();

        var result = (await playerQuestRepository.GetAll(playerId)).ToList();
        
        return Ok(new PlayerQuestPanelViewModel(result));
    }

    [HttpPost]
    [Route("task/complete")]
    public async Task<IActionResult> CompleteQuestTask([FromBody] CompleteQuestTaskRequest request)
    {
        await Task.Yield();

        return Ok();
    }

    [HttpPost]
    [Route("giver")]
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

    public class AcceptQuestRequest
    {
        public Guid QuestId { get; set; }
        public ulong PlayerId { get; set; }
        public long FactionId { get; set; }
        public Guid TerritoryId { get; set; }
        public int Seed { get; set; }
    }
    
    public class AbandonQuestRequest
    {
        public Guid QuestId { get; set; }
        public ulong PlayerId { get; set; }
    }

    public class AcceptQuestResponse(bool isSuccess, string message) : IOutcome
    {
        public bool IsSuccess { get; } = isSuccess;
        public string Message { get; } = message;

        public static AcceptQuestResponse Success(string message) => new(true, message);
        public static AcceptQuestResponse Failed(string message) => new(false, message);
    }

    public class CompleteQuestTaskRequest
    {
        public ulong ConstructId { get; set; }
        public ulong ElementId { get; set; }
        public ulong PlayerId { get; set; }
    }
    
    public class PlayerQuestPanelViewModel
    {
        public IEnumerable<QuestViewModel> Jobs { get; set; }

        public PlayerQuestPanelViewModel(IEnumerable<PlayerQuestItem> questItems)
        {
            Jobs = questItems
                .Select(pq => new QuestViewModel(pq))
                .OrderBy(q => q.Title);
        }
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
            Jobs = outcome.QuestList
                .Select(pq => new QuestViewModel(pq))
                .OrderBy(q => q.Title);
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

        public QuestViewModel(PlayerQuestItem item)
        {
            Id = item.Id;
            Title = item.Properties.Title;
            Type = item.Type;
            Tasks = item.TaskItems.Select(t => new QuestTaskViewModel(t));
            Rewards = item.Properties.RewardTextList;
        }
    }

    public class QuestTaskViewModel(QuestTaskItem questTaskItem)
    {
        public string Title { get; set; } = questTaskItem.Text;

        public string Position { get; set; } =
            $"::pos{{0,{questTaskItem.BaseConstruct ?? 0},{questTaskItem.Position.x}, {questTaskItem.Position.y}, {questTaskItem.Position.z}}}";

        public string Status { get; set; } = questTaskItem.Status;
    }

    public class GenerateQuestsRequest
    {
        public ulong PlayerId { get; set; }
        public long FactionId { get; set; }
        public Guid TerritoryId { get; set; }
        public int Seed { get; set; }
    }
}