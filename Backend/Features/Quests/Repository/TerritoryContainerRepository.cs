using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Quests.Data;
using Mod.DynamicEncounters.Features.Quests.Interfaces;

namespace Mod.DynamicEncounters.Features.Quests.Repository;

public class TerritoryContainerRepository(IServiceProvider provider) : ITerritoryContainerRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();
    
    public async Task<IEnumerable<TerritoryContainerItem>> GetAll(TerritoryId territoryId)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRow>(
            """
            SELECT * FROM public.mod_territory_quest_container
            WHERE territory_id = @territoryId
            """,
            new
            {
                territoryId = territoryId.Id
            }
        )).ToList();

        return result.Select(MapToModel);
    }

    public async Task Add(Guid territoryId, ulong constructId, ulong elementId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO public.mod_territory_quest_container (territory_id, construct_id, element_id)
            VALUES (
                    @territoryId,
                    @constructId,
                    @elementId
            )
            """,
            new
            {
                territoryId,
                constructId = (long)constructId,
                elementId = (long)elementId
            }
        );
    }

    private TerritoryContainerItem MapToModel(DbRow row)
    {
        return new TerritoryContainerItem
        {
            Id = row.id,
            TerritoryId = row.territory_id,
            ConstructId = row.construct_id,
            ElementId = row.element_id
        };
    }
    
    private struct DbRow
    {
        public Guid id { get; set; }
        public Guid territory_id { get; set; }
        public ulong construct_id { get; set; }
        public ulong element_id { get; set; }
    }
}