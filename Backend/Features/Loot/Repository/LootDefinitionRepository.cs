using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Loot.Data;
using Mod.DynamicEncounters.Features.Loot.Interfaces;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Loot.Repository;

public class LootDefinitionRepository(IServiceProvider provider) : ILootDefinitionRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();
    
    public async Task<IEnumerable<LootDefinitionItem>> GetAllActiveByTagsAsync(IEnumerable<string> tags)
    {
        using var db = _factory.Create();
        db.Open();

        var tagQuery = JsonConvert.SerializeObject(tags)
            .Replace("\"", "'");
        
        var result = (await db.QueryAsync<DbRow>(
            $"""
            SELECT * FROM mod_loot_def WHERE active = true AND
            tags ?| array{tagQuery}
            """
        )).ToList();

        if (result.Count == 0)
        {
            return [];
        }

        return result.Select(MapToModel);
    }

    private LootDefinitionItem MapToModel(DbRow row)
    {
        return new LootDefinitionItem
        {
            Id = row.id,
            Name = row.name,
            ElementRules = JsonConvert.DeserializeObject<IEnumerable<LootDefinitionItem.ElementReplacementRule>>(row.elements),
            ItemRules = JsonConvert.DeserializeObject<IEnumerable<LootDefinitionItem.LootItemRule>>(row.items),
            Tags = JsonConvert.DeserializeObject<IEnumerable<string>>(row.tags),
            CreatedAt = row.created_at,
            UpdatedAt = row.updated_at
        };
    }

    public struct DbRow
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string tags { get; set; }
        public string items { get; set; }
        public string elements { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public bool active { get; set; }
    }
}