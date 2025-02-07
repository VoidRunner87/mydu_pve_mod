using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Spawner.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IPrefabItemRepository
{
    Task<IEnumerable<PrefabItem>> GetAllAsync();
    Task AddAsync(PrefabItem model);
    Task DeleteAsync(Guid id);
    Task<PrefabItem?> FindAsync(string name);
}