using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

namespace Mod.DynamicEncounters.Threads.Handles;

public class HighPriority() : ConstructBehaviorLoop(10, BehaviorTaskCategory.HighPriority);