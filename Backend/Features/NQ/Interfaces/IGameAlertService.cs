using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.NQ.Interfaces;

public interface IGameAlertService
{
    Task PushInfoAlert(ulong playerId, string message);
    Task PushErrorAlert(ulong playerId, string message);
}