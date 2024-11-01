using System;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Party.Interfaces;

namespace Mod.DynamicEncounters.Features.Party.Data;

public class PartyCommandHandlerOutcome
{
    public bool Success { get; set; }
    public Func<IPlayerPartyService, Task<PartyOperationOutcome>> Action { get; set; }

    public static PartyCommandHandlerOutcome Failed(string message)
    {
        return new PartyCommandHandlerOutcome
        {
            Success = false,
            Action = _ => Task.FromResult(PartyOperationOutcome.Failed(message))
        };
    }

    public static PartyCommandHandlerOutcome Execute(Func<IPlayerPartyService, Task<PartyOperationOutcome>> action)
        => new()
        {
            Success = true,
            Action = action
        };
}