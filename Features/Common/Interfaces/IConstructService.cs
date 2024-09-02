using System.Threading.Tasks;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface IConstructService
{
    Task<ConstructInfo?> GetConstructInfoAsync(ulong constructId);
    Task SetDynamicWreckAsync(ulong constructId, bool isDynamicWreck);
}