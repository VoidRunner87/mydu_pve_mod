using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Data;
using Mod.DynamicEncounters.Features.Sector.Interfaces;

namespace Mod.DynamicEncounters.Features.Sector.Repository;

public class SectorEncounterRepository(IServiceProvider provider) : ISectorEncounterRepository
{
    private readonly IPostgresConnectionFactory _connectionFactory =
        provider.GetRequiredService<IPostgresConnectionFactory>();

    public Task AddAsync(SectorEncounterItem item)
    {
        throw new NotImplementedException();
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

    public async Task<IEnumerable<SectorEncounterItem>> FindActiveAsync()
    {
        using var db = _connectionFactory.Create();
        db.Open();

        var queryResult = await db.QueryAsync<DbRow>(
            "SELECT * FROM public.mod_sector_encounter WHERE active = true"
        );

        return queryResult.Select(DbRowToModel);
    }

    private static SectorEncounterItem DbRowToModel(DbRow first)
    {
        return new SectorEncounterItem
        {
            Id = first.id,
            Name = first.name,
            OnLoadScript = first.on_load_script,
            OnSectorEnterScript = first.on_sector_enter_script,
            Active = first.active
        };
    }

    private struct DbRow
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string on_load_script { get; set; }
        public string on_sector_enter_script { get; set; }
        public bool active { get; set; }
    }
}