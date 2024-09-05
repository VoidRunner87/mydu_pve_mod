using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.NQ.Interfaces;

public interface IPlayerService
{
    Task GrantPlayerTitleAsync(ulong playerId, string title);
}