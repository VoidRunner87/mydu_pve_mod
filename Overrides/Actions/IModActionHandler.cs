using System.Threading.Tasks;
using NQ;

namespace Mod.DynamicEncounters.Overrides.Actions;

public interface IModActionHandler
{
    Task HandleAction(ulong playerId, ModAction action);
}