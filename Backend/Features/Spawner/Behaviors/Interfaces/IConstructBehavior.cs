using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

public interface IConstructBehavior
{
    public BehaviorTaskCategory Category { get; }
    Task InitializeAsync(BehaviorContext context);
    Task TickAsync(BehaviorContext context);
}