using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Data;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Mod.DynamicEncounters.Helpers;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Sector.Repository;

public class SectorEncounterRepository(IServiceProvider provider) : ISectorEncounterRepository
{
    private readonly IPostgresConnectionFactory _connectionFactory =
        provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task AddAsync(SectorEncounterItem item)
    {
        using var db = _connectionFactory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO mod_sector_encounter (id, name, on_load_script, on_sector_enter_script, active, json_properties)
            VALUES (@Id, @Name, @OnLoadScript, @OnSectorEnterScript, @Active, @json_properties::jsonb)
            """,
            new
            {
                item.Id,
                item.Name,
                item.OnLoadScript,
                item.OnSectorEnterScript,
                item.Active,
                json_properties = JsonConvert.SerializeObject(item.Properties)
            }
        );
    }

    public Task SetAsync(IEnumerable<SectorEncounterItem> items)
    {
        throw new NotImplementedException();
    }

    public Task AddRangeAsync(IEnumerable<SectorEncounterItem> items)
    {
        throw new NotImplementedException();
    }

    public Task<SectorEncounterItem?> FindAsync(object key)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<SectorEncounterItem>> GetAllAsync()
    {
        using var db = _connectionFactory.Create();
        db.Open();

        var queryResult = await db.QueryAsync<DbRow>(
            "SELECT * FROM public.mod_sector_encounter"
        );

        return queryResult.Select(DbRowToModel);
    }

    public Task<long> GetCountAsync()
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(object key)
    {
        throw new NotImplementedException();
    }

    public Task Clear()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<SectorEncounterItem>> FindActiveTaggedAsync(string tag)
    {
        using var db = _connectionFactory.Create();
        db.Open();

        var queryResult = await db.QueryAsync<DbRow>(
            "SELECT * FROM public.mod_sector_encounter WHERE active = true AND json_properties->'Tags' @> @tag::jsonb",
            new { tag = tag.AsJsonB() }
        );

        return queryResult.Select(DbRowToModel);
    }

    public async Task<IEnumerable<SectorEncounterItem>> FindActiveByFactionAsync(long factionId)
    {
        using var db = _connectionFactory.Create();
        db.Open();

        var queryResult = await db.QueryAsync<DbRow>(
            "SELECT * FROM public.mod_sector_encounter WHERE active = true AND faction_id = @factionId",
            new { factionId }
        );

        return queryResult.Select(DbRowToModel);
    }

    private static SectorEncounterItem DbRowToModel(DbRow row)
    {
        return new SectorEncounterItem
        {
            Id = row.id,
            Name = row.name,
            OnLoadScript = row.on_load_script,
            OnSectorEnterScript = row.on_sector_enter_script,
            Active = row.active,
            Tag = row.tag,
            TerritoryId = row.territory_id,
            RestrictToOwnedTerritory = row.restrict_to_owned_territory,
            Properties = JsonConvert.DeserializeObject<EncounterProperties>(row.json_properties)
        };
    }

    private struct DbRow
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string on_load_script { get; set; }
        public string on_sector_enter_script { get; set; }
        public bool active { get; set; }
        public string json_properties { get; set; }
        public string tag { get; set; }
        public Guid territory_id { get; set; }
        public bool restrict_to_owned_territory { get; set; }
    }
}