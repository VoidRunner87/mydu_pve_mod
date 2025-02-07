using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Faction.Data;

namespace Mod.DynamicEncounters.Features.Faction.Interfaces;

public interface IFactionNameRepository
{
    Task<string> GetRandomFactionNameByGroup(FactionId factionId, string groupName);
}