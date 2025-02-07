using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.NQ.Interfaces;

public interface IWalletService
{
    Task AddToPlayerWallet(ulong playerId, ulong amount);
}