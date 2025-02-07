using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.Commands.Interfaces;

public interface INpcKillsCommandHandler
{
    Task HandleCommand(ulong instigatorPlayerId, string command);
}