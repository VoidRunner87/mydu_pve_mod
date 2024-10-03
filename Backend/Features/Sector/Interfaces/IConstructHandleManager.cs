using System.Threading.Tasks;
using NQ;

namespace Mod.DynamicEncounters.Features.Sector.Interfaces;

public interface IConstructHandleManager
{
    Task CleanupConstructHandlesInSectorAsync(Vec3 sector);
    Task CleanupConstructsThatFailedSectorCleanupAsync();
    Task TagAsDeletedConstructHandledThatAreDeletedConstructs();
}