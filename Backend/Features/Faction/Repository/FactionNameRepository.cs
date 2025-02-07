using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Faction.Data;
using Mod.DynamicEncounters.Features.Faction.Interfaces;

namespace Mod.DynamicEncounters.Features.Faction.Repository;

public class FactionNameRepository(IServiceProvider provider) : IFactionNameRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();
    
    public async Task<string> GetRandomFactionNameByGroup(FactionId factionId, string groupName)
    {
        using var db = _factory.Create();
        db.Open();

        return await db.ExecuteScalarAsync<string>(
            """
            SELECT name FROM public.mod_faction_name 
            WHERE "group" = @group AND faction_id = @faction_id
            ORDER BY RANDOM()
            LIMIT 1
            """,
            new
            {
                faction_id = factionId.Id,
                group = groupName
            }
        );
    }
}