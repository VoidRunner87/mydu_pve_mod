using System;
using System.Linq;
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

        var titles = await db.ExecuteScalarAsync<string>(
            """
            SELECT titles FROM auth WHERE display_name = (SELECT display_name FROM player WHERE id = @playerId)
            """,
            new
            {
                playerId = (long)playerId
            }
        );

        var titleSet = titles.Split(",")
            .ToHashSet();

        titleSet.Add(title);

        var titleList = titleSet.ToList().OrderBy(x => x);

        await db.ExecuteAsync(
            """
            UPDATE auth 
            SET titles = @titles 
            WHERE display_name = (SELECT display_name FROM player WHERE id = @playerId)
            """,
            new
            {
                titles = string.Join(",", titleList),
                playerId = (long)playerId
            }
        );
        
        var count = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(0) FROM public.player_title WHERE player_id = @playerId AND title = @title",
            new
            {
                playerId = (long)playerId, 
                title
            }
        );

        if (count > 0)
        {
            return;
        }

        await db.ExecuteAsync(
            """
            INSERT INTO public.player_title (player_id, title) VALUES(@playerId, @title)          
            """,
            new
            {
                playerId = (long)playerId,
                title
            }
        );
    }
}