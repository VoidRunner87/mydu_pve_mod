using System;
using Mod.DynamicEncounters.Features.Quests.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestTaskItem(
    Guid id,
    string text,
    string type,
    string status,
    Vec3 position,
    ScriptActionItem onCheckScript,
    IQuestTaskItemDefinition definition)
{
    public Guid Id { get; } = id;
    public string Text { get; } = text;
    public string Type { get; } = type;
    public string Status { get; } = status;
    public ulong? BaseConstruct { get; set; } = 0;
    public Vec3 Position { get; } = position;
    public ScriptActionItem OnCheckScript { get; } = onCheckScript;
    public IQuestTaskItemDefinition Definition { get; } = definition;
}