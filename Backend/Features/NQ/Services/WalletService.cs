using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.NQ.Services;

public class WalletService(IServiceProvider provider) : IWalletService
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();
    
    public async Task AddToPlayerWallet(ulong playerId, ulong amount)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            "UPDATE player SET wallet = wallet + @amount WHERE id = @playerId",
            new
            {
                playerId = (long)playerId,
                amount = (long)amount
            }
        );
    }
}