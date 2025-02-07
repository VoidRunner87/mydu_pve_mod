using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Data;
using Mod.DynamicEncounters.Features.Market.Interfaces;

namespace Mod.DynamicEncounters.Features.Market.Data;

public class OrePriceRepository(IServiceProvider provider) : IOrePriceRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task<Dictionary<string, Quanta>> GetOrePrices()
    {
        using var db = _factory.Create();
        db.Open();

        var rows = await db.QueryAsync<DbRow>(
            """
            SELECT
                I.name,
                TRUNC(AVG(price)) AS price
            FROM public.market_order MO
            INNER JOIN item_definition I ON (I.id = MO.item_type_id)
            WHERE 
                parent_id IN (1240631464, 1240631465, 1240631466, 1240631467, 1240631468)
                AND update_date >= CURRENT_DATE - INTERVAL '30 days'
                AND update_date < CURRENT_DATE + INTERVAL '1 day'
            GROUP BY I.name;
            """
        );

        return MapToModel(rows);
    }

    private static Dictionary<string, Quanta> MapToModel(IEnumerable<DbRow> rows)
    {
        var orePrices = GetDefaultOrePrices();
        
        foreach (var row in rows)
        {
            var quanta = new Quanta((long)row.price);
            if (!orePrices.TryAdd(row.name, quanta))
            {
                orePrices[row.name] = quanta;
            }
        }

        return orePrices;
    }

    private static Dictionary<string, Quanta> GetDefaultOrePrices()
    {
        return new Dictionary<string, Quanta>
        {
            { "AluminiumOre", 2500 },
            { "CarbonOre", 2500 },
            { "IronOre", 2500 },
            { "SiliconOre", 2500 },
            { "CalciumOre", 20000 },
            { "ChromiumOre", 20000 },
            { "CopperOre", 20000 },
            { "SodiumOre", 20000 },
            { "LithiumOre", 60000 },
            { "NickelOre", 46000 },
            { "SilverOre", 51000 },
            { "SulfurOre", 11000 },
            { "CobaltOre", 140000 },
            { "FluorineOre", 9500 },
            { "GoldOre", 190000 },
            { "ScandiumOre", 135000 },
            { "ManganeseOre", 135000 },
            { "NiobiumOre", 88800 },
            { "TitaniumOre", 1800000 },
            { "VanadiumOre", 700000 },
            { "ThoramineOre", 20000000 },
        };
    }

    private readonly struct DbRow
    {
        public string name { get; init; }
        public double price { get; init; }
    }
}