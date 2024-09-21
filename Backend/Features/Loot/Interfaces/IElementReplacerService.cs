using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.Loot.Interfaces;

public interface IElementReplacerService
{
    Task ReplaceSingleElementAsync(ulong constructId, string elementTypeName, string withElementTypeName);
}