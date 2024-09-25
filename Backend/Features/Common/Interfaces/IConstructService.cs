using System;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface IConstructService
{
    Task<ConstructInfo?> GetConstructInfoAsync(ulong constructId);
    Task ResetConstructCombatLock(ulong constructId);
    Task SetDynamicWreckAsync(ulong constructId, bool isDynamicWreck);
    Task<Velocities> GetConstructVelocities(ulong constructId);
    Task DeleteAsync(ulong constructId);
    Task SetAutoDeleteFromNow(ulong constructId, TimeSpan timeSpan);
}