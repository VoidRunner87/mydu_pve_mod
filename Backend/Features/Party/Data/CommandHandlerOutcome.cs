using System;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Party.Interfaces;

namespace Mod.DynamicEncounters.Features.Party.Data;

public class CommandHandlerOutcome
{
    public bool Success { get; set; }
    public Func<IPlayerPartyService, Task<PartyOperationOutcome>> Action { get; set; }

    public static CommandHandlerOutcome Failed(string message)
    {
        return new CommandHandlerOutcome
        {
            Success = false,
            Action = _ => Task.FromResult(PartyOperationOutcome.Failed(message))
        };
    }

    public static CommandHandlerOutcome Execute(Func<IPlayerPartyService, Task<PartyOperationOutcome>> action)
        => new()
        {
            Success = true,
            Action = action
        };
}