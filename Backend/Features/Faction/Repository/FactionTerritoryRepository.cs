using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Faction.Interfaces;

namespace Mod.DynamicEncounters.Features.Faction.Repository;

public class FactionTerritoryRepository(IServiceProvider provider) : IFactionTerritoryRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task<IEnumerable<FactionTerritoryItem>> GetAllByFactionAsync(long factionId)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            """
            SELECT * FROM public.mod_faction_territory WHERE faction_id = @factionId
            """,
            new
            {
                factionId
            }
        )).ToList();

        return result.Select(MapToModel);
    }

    private FactionTerritoryItem MapToModel(DbRow row)
    {
        return new FactionTerritoryItem
        {
            Id = row.id,
            TerritoryId = row.territory_id,
            FactionId = row.faction_id,
            IsActive = row.active,
            IsPermanent = row.permanent,
            UpdatedAt = row.updated_at,
            SectorCount = row.sector_count
        };
    }

    public struct DbRow
    {
        public Guid id;
        public long faction_id;
        public Guid territory_id;
        public bool permanent;
        public bool active;
        public int sector_count;
        public DateTime updated_at;
    }
}