using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Data;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using Newtonsoft.Json;
using NQ;

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

    public Task UpdateAsync(SectorEncounterItem item)
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

        var queryResult = await db.QueryAsync<DbRowWithTerritoryJoin>(
            """
            SELECT 
                E.id,
                E.name,
                E.on_load_script,
                E.on_sector_enter_script,
                E.active,
                E.faction_id,
                T.spawn_position_x,
                T.spawn_position_y,
                T.spawn_position_z,
                T.spawn_min_radius,
                T.spawn_max_radius,
                T.spawn_expiration_span,
                T.active territory_active,
                T.id territory_id
            FROM public.mod_sector_encounter AS E
            INNER JOIN public.mod_territory AS T ON (T.id = E.territory_id)
            """
        );

        return queryResult.Select(DbRowWithTerritoryToModel);
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

    public async Task<IEnumerable<SectorEncounterItem>> FindActiveByFactionAsync(long factionId)
    {
        using var db = _connectionFactory.Create();
        db.Open();

        var queryResult = await db.QueryAsync<DbRowWithTerritoryJoin>(
            """
            SELECT 
                E.id,
                E.name,
                E.on_load_script,
                E.on_sector_enter_script,
                E.active,
                E.faction_id,
                T.spawn_position_x,
                T.spawn_position_y,
                T.spawn_position_z,
                T.spawn_min_radius,
                T.spawn_max_radius,
                T.spawn_expiration_span,
                T.active territory_active,
                T.id territory_id
            FROM public.mod_sector_encounter AS E
            INNER JOIN public.mod_territory AS T ON (T.id = E.territory_id)
            WHERE E.active IS TRUE AND E.faction_id = @factionId AND
                  T.active IS TRUE
            """,
            new { factionId }
        );

        return queryResult.Select(DbRowWithTerritoryToModel);
    }

    public async Task<IEnumerable<SectorEncounterItem>> FindActiveByFactionTerritoryAsync(long factionId, Guid territoryId)
    {
        using var db = _connectionFactory.Create();
        db.Open();

        var queryResult = await db.QueryAsync<DbRowWithTerritoryJoin>(
            """
            SELECT 
                E.id,
                E.name,
                E.on_load_script,
                E.on_sector_enter_script,
                E.active,
                E.faction_id,
                T.spawn_position_x,
                T.spawn_position_y,
                T.spawn_position_z,
                T.spawn_min_radius,
                T.spawn_max_radius,
                T.spawn_expiration_span,
                T.active territory_active,
                T.id territory_id
            FROM public.mod_sector_encounter AS E
            INNER JOIN public.mod_territory AS T ON (T.id = E.territory_id)
            INNER JOIN public.mod_faction_territory AS FT ON (FT.faction_id = E.faction_id AND FT.territory_id = T.id)
            WHERE E.active IS TRUE AND E.faction_id = @factionId AND
                  T.active IS TRUE AND T.id = @territoryId AND
                  FT.active IS TRUE
            """,
            new { factionId, territoryId }
        );

        return queryResult.Select(DbRowWithTerritoryToModel);
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
    
    private static SectorEncounterItem DbRowWithTerritoryToModel(DbRowWithTerritoryJoin row)
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
            Properties =
            {
                CenterPosition = new Vec3
                {
                    x = row.spawn_position_x,
                    y = row.spawn_position_y,
                    z = row.spawn_position_z,
                },
                MaxRadius = row.spawn_max_radius,
                MinRadius = row.spawn_min_radius,
                ExpirationTimeSpan = row.spawn_expiration_span,
            }
        };
    }

    private class DbRow
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

    private class DbRowWithTerritoryJoin : DbRow
    {
        public double spawn_position_x { get; set; }
        public double spawn_position_y { get; set; }
        public double spawn_position_z { get; set; }
        public double spawn_min_radius { get; set; }
        public double spawn_max_radius { get; set; }
        public TimeSpan spawn_expiration_span { get; set; }
        public bool territory_active { get; set; }
    }
}