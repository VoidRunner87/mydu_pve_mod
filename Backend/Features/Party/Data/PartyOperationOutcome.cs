using Mod.DynamicEncounters.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Party.Data;

public class PartyOperationOutcome : IOutcome
{
    public bool Success { get; init; }
    public string Message { get; init; }

    public PlayerPartyGroupId? PartyGroupId { get; init; }

    public static PartyOperationOutcome Successful(PlayerPartyGroupId partyGroupId, string message)
        => new() { Success = true, Message = message, PartyGroupId = partyGroupId };
    
    public static PartyOperationOutcome AlreadyAccepted(PlayerPartyGroupId partyGroupId)
        => new() { Success = true, Message = "Already accepted", PartyGroupId = partyGroupId };
    
    public static PartyOperationOutcome Disbanded(PlayerPartyGroupId partyGroupId)
        => new() { Success = true, Message = "Group disbanded", PartyGroupId = partyGroupId };

    public static PartyOperationOutcome Failed(string message)
        => new() { Success = false, Message = message };

    public static PartyOperationOutcome AlreadyInAParty()
        => new() { Success = false, Message = "Already in a group" };
    
    public static PartyOperationOutcome PlayerNotInAParty()
        => new() { Success = false, Message = "Player selected not in a group" };
    
    public static PartyOperationOutcome NotAnAcceptedMember()
        => new() { Success = false, Message = "Player selected is pending invite or pending being accepted to join" };
    
    public static PartyOperationOutcome PlayerOnDifferentParties()
        => new() { Success = false, Message = "Players are on different groups" };
    
    public static PartyOperationOutcome InvalidRole()
        => new() { Success = false, Message = "Invalid role" };
    
    public static PartyOperationOutcome MustBePartyLeaderToDisband()
        => new() { Success = false, Message = "Must be a group leader to disband" };
    
    public static PartyOperationOutcome MustBePartyLeaderPromoteAnotherPlayer()
        => new() { Success = false, Message = "Must be a group leader to promote another player" };
    
    public static PartyOperationOutcome MustBePartyLeader()
        => new() { Success = false, Message = "Must be a group leader" };
}