using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Party.Data;

public class PartyOperationOutcome : IOutcome
{
    public bool Success { get; init; }
    public string Message { get; init; }

    public PlayerPartyGroupId? PartyGroupId { get; init; }

    public static PartyOperationOutcome Successful(PlayerPartyGroupId partyGroupId, string message)
        => new() { Success = true, Message = message, PartyGroupId = partyGroupId };
    
    public static PartyOperationOutcome Disbanded(PlayerPartyGroupId partyGroupId)
        => new() { Success = true, Message = "", PartyGroupId = partyGroupId };

    public static PartyOperationOutcome Failed(string message)
        => new() { Success = false, Message = message };

    public static PartyOperationOutcome AlreadyInAParty()
        => new() { Success = false, Message = "Already in a party" };
    
    public static PartyOperationOutcome PlayerNotInAParty()
        => new() { Success = false, Message = "Player selected not in a party" };
    
    public static PartyOperationOutcome PlayerOnDifferentParties()
        => new() { Success = false, Message = "Players are on different parties" };
    
    public static PartyOperationOutcome MustBePartyLeaderToDisband()
        => new() { Success = false, Message = "Must be a party leader to disband" };
    
    public static PartyOperationOutcome MustBePartyLeaderPromoteAnotherPlayer()
        => new() { Success = false, Message = "Must be a party leader to promote another player" };
}