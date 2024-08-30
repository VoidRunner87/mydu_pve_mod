using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.Sector.Interfaces;

public interface IConstructHandleManager
{
    Task CleanupExpiredConstructHandlesAsync();
}