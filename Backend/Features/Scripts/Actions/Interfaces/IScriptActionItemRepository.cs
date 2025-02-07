using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IScriptActionItemRepository
{
    Task<bool> ActionExistAsync(string actionName);
    Task<ScriptActionItem?> FindAsync(string name);
    Task AddAsync(ScriptActionItem script);
    Task<IEnumerable<ScriptActionItem>> GetAllAsync();
    Task UpdateAsync(ScriptActionItem item);
    Task DeleteAsync(Guid id);
}