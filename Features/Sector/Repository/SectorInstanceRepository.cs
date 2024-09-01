using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Sector.Data;
using Mod.DynamicEncounters.Features.Sector.Interfaces;
using NQ;
using NQutils.Sql;

namespace Mod.DynamicEncounters.Features.Sector.Repository;

public class SectorInstanceRepository(IServiceProvider provider) : ISectorInstanceRepository
{
    private readonly IPostgresConnectionFactory _connectionFactory =
        provider.GetRequiredService<IPostgresConnectionFactory>();

    private readonly ISql _sql = provider.GetRequiredService<ISql>();

    public async Task AddAsync(SectorInstance item)
    {
        using var db = _connectionFactory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO public.mod_sector_instance (id, sector_x, sector_y, sector_z, expires_at, on_load_script, on_sector_enter_script)
            VALUES (@Id, @PosX, @PosY, @PosZ, @ExpiresAt, @OnLoadScript, @OnSectorEnterScript);
            """,
            new
            {
                item.Id,
                PosX = item.Sector.x,
                PosY = item.Sector.y,
                PosZ = item.Sector.z,
                item.ExpiresAt,
                item.OnLoadScript,
                item.OnSectorEnterScript
            }
        );
    }

    public Task SetAsync(IEnumerable<SectorInstance> items)
    {
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<SectorInstance> items)
    {
        return Task.CompletedTask;
    }

    public async Task<SectorInstance?> FindAsync(object key)
    {
        var result = await _sql.Query<DbRow>("SELECT * FROM public.mod_sector_instance WHERE id = @id", (Guid)key);

        if (result.Count > 0)
        {
            var first = result.First();

            return MapToModel(first);
        }

        return null;
    }

    private static SectorInstance MapToModel(DbRow first)
    {
        return new SectorInstance
        {
            Id = first.id,
            ExpiresAt = first.expires_at,
            OnLoadScript = first.on_load_script,
            OnSectorEnterScript = first.on_sector_enter_script,
            StartedAt = first.started_at,
            Sector = new Vec3
            {
                x = first.sector_x,
                y = first.sector_y,
                z = first.sector_z,
            }
        };
    }

    public async Task<IEnumerable<SectorInstance>> GetAllAsync()
    {
        using var db = _connectionFactory.Create();
        db.Open();

        var queryResult = await db.QueryAsync<DbRow>("SELECT * FROM public.mod_sector_instance");

        return queryResult.Select(MapToModel);
    }

    public async Task<long> GetCountAsync()
    {
        using var db = _connectionFactory.Create();
        db.Open();

        return await db.ExecuteScalarAsync<long>("SELECT COUNT(0) FROM public.mod_sector_instance");
    }

    public async Task DeleteAsync(object key)
    {
        using var db = _connectionFactory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            DELETE FROM public.mod_sector_instance WHERE id = @id
            """,
            new
            {
                id = key
            }
        );
    }

    public async Task<SectorInstance?> FindBySector(Vec3 sector)
    {
        using var db = _connectionFactory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            """
            SELECT * FROM public.mod_sector_instance WHERE sector_x = @x AND sector_y = @y AND sector_z = @z
            """,
            new
            {
                sector.x,
                sector.y,
                sector.z,
            }
        )).ToList();

        if (!result.Any())
        {
            return null;
        }

        return MapToModel(result[0]);
    }

    public async Task<IEnumerable<SectorInstance>> FindExpiredAsync()
    {
        using var db = _connectionFactory.Create();
        db.Open();

        var queryResult =
            await db.QueryAsync<DbRow>("SELECT * FROM public.mod_sector_instance WHERE expires_at < NOW()");

        return queryResult.Select(MapToModel);
    }

    public async Task DeleteExpiredAsync()
    {
        using var db = _connectionFactory.Create();
        db.Open();

        await db.ExecuteScalarAsync("DELETE FROM public.mod_sector_instance WHERE expires_at < NOW()");
    }

    public async Task ExtendExpirationAsync(Guid id, int minutes)
    {
        using var db = _connectionFactory.Create();
        db.Open();

        await db.ExecuteAsync(
            $"""
             UPDATE public.mod_sector_instance SET expires_at = NOW() + INTERVAL '{minutes} minutes'
             WHERE id = @id
             """,
            new
            {
                id
            }
        );
    }

    public async Task<IEnumerable<SectorInstance>> FindUnloadedAsync()
    {
        using var db = _connectionFactory.Create();
        db.Open();

        var queryResult =
            await db.QueryAsync<DbRow>("SELECT * FROM public.mod_sector_instance WHERE loaded_at IS NULL");

        return queryResult.Select(MapToModel);
    }

    public async Task SetLoadedAsync(Guid id, bool loaded)
    {
        using var db = _connectionFactory.Create();
        db.Open();

        if (loaded)
        {
            await db.ExecuteAsync("UPDATE public.mod_sector_instance SET loaded_at = NOW() WHERE id = @id", new { id });
        }
        else
        {
            await db.ExecuteAsync("UPDATE public.mod_sector_instance SET loaded_at = NULL WHERE id = @id", new { id });
        }
    }

    public async Task TagAsStartedAsync(Guid id)
    {
        using var db = _connectionFactory.Create();
        db.Open();

        await db.ExecuteAsync("UPDATE public.mod_sector_instance SET started_at = NOW() WHERE id = @id", new { id });
    }

    public async Task<IEnumerable<SectorInstance>> FindSectorsRequiringStartupAsync()
    {
        using var db = _connectionFactory.Create();
        db.Open();

        var queryResult = await db.QueryAsync<DbRow>(
            """
            SELECT si.* FROM public.mod_sector_instance AS si
                     INNER JOIN public.construct AS c ON (c.sector_x = si.sector_x AND c.sector_y = si.sector_y AND c.sector_z = si.sector_z)
                     WHERE si.started_at IS NULL AND 
                           c.owner_entity_id IS NOT NULL
            """
        );

        return queryResult.Select(MapToModel);
    }

    private struct DbRow
    {
        public Guid id { get; set; }
        public long sector_x { get; set; }
        public long sector_y { get; set; }
        public long sector_z { get; set; }
        public string on_load_script { get; set; }
        public string on_sector_enter_script { get; set; }
        public DateTime expires_at { get; set; }
        public DateTime? started_at { get; set; }
    }
}