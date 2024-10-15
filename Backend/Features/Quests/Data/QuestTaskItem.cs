using System;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Quests.Data;

public class QuestTaskItem(
    Guid id,
    string text,
    string type,
    Vec3 position,
    ScriptActionItem? onSuccessScript,
    ScriptActionItem? onFailureScript,
    IQuestTaskItemDefinition definition)
{
    public Guid Id { get; } = id;
    public string Text { get; } = text;
    public string Type { get; } = type;
    public Vec3 Position { get; } = position;
    public ScriptActionItem? OnSuccessScript { get; } = onSuccessScript;
    public ScriptActionItem? OnFailureScript { get; } = onFailureScript;
    public IQuestTaskItemDefinition Definition { get; } = definition;
}

public interface IQuestTaskItemDefinition
{
    
}

public abstract class TransportItemTaskDefinition(TerritoryContainerItem container) : IQuestTaskItemDefinition
{
    public TerritoryContainerItem Container { get; } = container;
}

public class PickupItemTaskItemDefinition(TerritoryContainerItem container) : TransportItemTaskDefinition(container)
{
    
}

public class DropItemTaskDefinition(TerritoryContainerItem container) : TransportItemTaskDefinition(container)
{
    
}

