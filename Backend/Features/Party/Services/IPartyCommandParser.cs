using Mod.DynamicEncounters.Features.Party.Data;

namespace Mod.DynamicEncounters.Features.Party.Services;

public interface IPartyCommandParser
{
    CommandHandlerOutcome Parse(ulong instigatorPlayerId, string command);
}