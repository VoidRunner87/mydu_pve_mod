using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface IAsteroidService
{
    Task HideFromDsatListAsync(ulong constructId);
}