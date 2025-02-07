using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

public interface IConstructStateService
{
    Task<ConstructStateOutcome> PersistState(ConstructStateItem stateItem);
    Task<ConstructStateOutcome> Find(string type, ulong constructId);
}