using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.Commands.Interfaces;

public interface IOpenPlayerBoardCommandHandler
{
    Task HandleCommand(ulong instigatorPlayerId, string command);
}