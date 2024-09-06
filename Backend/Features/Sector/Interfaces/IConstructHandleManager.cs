using System.Threading.Tasks;
using BotLib.BotClient;
using NQ;

namespace Mod.DynamicEncounters.Features.Sector.Interfaces;

public interface IConstructHandleManager
{
    Task CleanupExpiredConstructHandlesAsync(Client client, Vec3 sector);
    Task CleanupConstructHandlesInSectorAsync(Client client, Vec3 sector);
}