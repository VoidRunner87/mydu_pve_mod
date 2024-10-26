using System.Threading.Tasks;
using Mod.DynamicEncounters.Overrides.Common.Data;

namespace Mod.DynamicEncounters.Overrides.Common.Interfaces;

public interface IPlayerService
{
    Task<PlayerPosition?> GetPlayerPositionCached(ulong playerId);
}