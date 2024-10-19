using System.Threading.Tasks;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Services;

public interface IPlayerAlertService
{
    Task SendErrorAlert(PlayerId playerId, string message);
    Task SendInfoAlert(PlayerId playerId, string message);
    Task SendNetworkNotification(PlayerId playerId, string message, int delay);
}