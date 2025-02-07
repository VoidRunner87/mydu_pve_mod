using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

namespace Mod.DynamicEncounters.Threads.Handles;

public class MovementPriority() : ConstructBehaviorLoop(20, BehaviorTaskCategory.MovementPriority, true);