using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.Party.Interfaces;

public interface IPlayerPartyCommandHandler
{
    Task HandleCommand(ulong instigatorPlayerId, string command);
}