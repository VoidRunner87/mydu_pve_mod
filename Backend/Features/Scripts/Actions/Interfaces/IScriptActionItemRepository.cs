using System.Threading.Tasks;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IScriptActionItemRepository : IRepository<ScriptActionItem>
{
    Task<bool> ActionExistAsync(string actionName);
}