using System;
using System.Collections.Generic;

namespace Mod.DynamicEncounters.Overrides.Actions.Data;

public class PartyData
{
    public Guid GroupId { get; set; }

    public PartyMemberEntry Leader { get; set; }
    public IEnumerable<PartyMemberEntry> Members { get; set; } = [];
    public IEnumerable<PartyMemberEntry> Invited { get; set; } = [];
    public IEnumerable<PartyMemberEntry> PendingAccept { get; set; } = [];
    
    public class PartyMemberEntry
    {
        public bool IsLeader { get; set; }
        public bool IsConnected { get; set; }
        public string PlayerName { get; set; } = "";
        public ConstructData? Construct { get; set; }
        public string Role { get; set; }
        public string Theme { get; set; }

        public class ConstructData
        {
            public ulong ConstructId { get; set; }
            public string ConstructName { get; set; } = "";
            public long Size { get; set; }
            public double ShieldRatio { get; set; }
            public double CoreStressRatio { get; set; }
        }
    }
}