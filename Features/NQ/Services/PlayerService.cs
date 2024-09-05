using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.NQ.Services;

public class PlayerService(IServiceProvider provider) : IPlayerService
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();
    
    public async Task GrantPlayerTitleAsync(ulong playerId, string title)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO public.player_title (player_id, title) VALUES(@playerId, @title)          
            """,
            new { playerId, title }
        );
    }
}