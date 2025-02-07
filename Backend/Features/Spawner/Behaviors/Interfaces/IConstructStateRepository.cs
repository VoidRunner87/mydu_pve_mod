using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

public interface IConstructStateRepository
{
    Task<ConstructStateItem?> Find(ulong constructId, string type);
    Task Add(ConstructStateItem item);
    Task Update(ConstructStateItem item);
}