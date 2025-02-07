using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Data;

namespace Mod.DynamicEncounters.Features.Spawner.Behaviors.Skills.Interfaces;

public interface IJamTargetService
{
    Task<JamTargetOutcome> JamAsync(JamConstructCommand command);
}