using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.NQ.Services;

public partial class PlayerService(IServiceProvider provider) : IPlayerService
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task<IEnumerable<ulong>> GetAllPlayersActiveOnInterval(TimeSpan timeSpan)
    {
        using var db = _factory.Create();
        db.Open();
        
        var playerIdList = await db.QueryAsync<ulong>(
            $"""
            SELECT * FROM public.player WHERE last_connection > NOW() - INTERVAL '{timeSpan.ToPostgresInterval()}'
            """
        );

        return playerIdList;
    }

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

    public async Task GivePlayerElementSkins(ulong playerId, IEnumerable<IPlayerService.ElementSkinItem> skinItems)
    {
        var skinItemsSanitized = skinItems
            .Select(x => x with { Skin = ReplaceInvalidChars().Replace(x.Skin, "") })
            .ToList();

        using var db = _factory.Create();
        db.Open();

        var values = skinItemsSanitized.Select(x => $"({playerId}, {x.ElementTypeId}, '{x.Skin}')");
        var insertValues = string.Join(",\n", values);

        await db.ExecuteAsync(
            $"""
             INSERT INTO public.player_skins (player_id, item_type, name)
             VALUES
                  {insertValues}
             """
        );
    }

    public async Task<Dictionary<ulong, HashSet<string>>> GetAllElementSkins(ulong playerId)
    {
        using var db = _factory.Create();
        db.Open();

        var items = (await db.QueryAsync<ElementSkinRow>(
            "SELECT * FROM player_skins WHERE player_id = @playerId",
            new { playerId = (long)playerId }
        ));

        var map = new Dictionary<ulong, HashSet<string>>();

        foreach (var item in items)
        {
            var elementTypeId = (ulong)item.item_type;
            if (!map.TryAdd(elementTypeId, [item.name]))
            {
                map[elementTypeId].Add(item.name);
            }
        }

        return map;
    }

    public struct ElementSkinRow
    {
        public long item_type { get; set; }
        public string name { get; set; }
    }

    [GeneratedRegex("[^a0-z9]", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex ReplaceInvalidChars();
}