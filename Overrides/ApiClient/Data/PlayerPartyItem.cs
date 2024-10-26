using System;

namespace Mod.DynamicEncounters.Overrides.ApiClient.Data;

public class PlayerPartyItem
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public ulong PlayerId { get; set; }
    public bool IsLeader { get; set; }
    public bool IsPendingAcceptInvite { get; set; }
    public bool IsPendingAcceptRequest { get; set; }

    public PartyProperties Properties { get; set; } = new();
    public DateTime CreatedAt { get; set; }

    public class PartyProperties
    {
        public string Theme { get; set; } = "default";
        public string Role { get; set; }
    }
}