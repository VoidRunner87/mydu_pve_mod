﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.NQ.Interfaces;

public interface IPlayerService
{
    Task<IEnumerable<ulong>> GetAllPlayersActiveOnInterval(TimeSpan timeSpan);
    
    Task GrantPlayerTitleAsync(ulong playerId, string title);

    Task GivePlayerElementSkins(ulong playerId, IEnumerable<ElementSkinItem> skinItems);

    Task<Dictionary<ulong, HashSet<string>>> GetAllElementSkins(ulong playerId);
    
    public struct ElementSkinItem
    {
        public ulong ElementTypeId { get; set; }
        public string Skin { get; set; }
    }
}