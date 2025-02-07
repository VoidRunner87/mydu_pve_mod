using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.Commands.Interfaces;

public interface IWarpAnchorCommandHandler
{
    Task HandleCommand(ulong instigatorPlayerId, string command);
}