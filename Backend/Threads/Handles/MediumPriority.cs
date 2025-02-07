using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

namespace Mod.DynamicEncounters.Threads.Handles;

public class MediumPriority() : ConstructBehaviorLoop(1, BehaviorTaskCategory.MediumPriority);