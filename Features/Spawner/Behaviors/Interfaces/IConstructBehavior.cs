using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

public interface IConstructBehavior
{
    bool IsActive();
    Task InitializeAsync(BehaviorContext context);
    Task TickAsync(BehaviorContext context);
}