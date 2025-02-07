using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.ExtendedProperties.Data;
using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Repository;

public class TraitRepository(IServiceProvider provider) : ITraitRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>(); 
    
    public async Task<ITraitCollection> Get()
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRowTraitProp>(
            """
            SELECT T.name trait_name, T.description trait_description, TP.*, TP.default_value as value 
            FROM public.mod_trait_properties TP
            INNER JOIN public.mod_trait T ON (T.id = TP.trait_id)
            """
        )).ToList();

        var traitMap = new Dictionary<string, ITrait>();
        
        foreach (var row in result)
        {
            var trait = MapTrait(row);
            var prop = MapTraitProperties(row);

            traitMap.TryAdd(row.trait_name, trait);
            traitMap[row.trait_name].Properties.TryAdd(row.name, prop);
        }

        return new TraitCollection(
            traitMap
        );
    }
    
    public async Task<ITraitCollection> GetElementTraits(string elementTypeName)
    {
        using var db = _factory.Create();
        db.Open();

        var result = (await db.QueryAsync<DbRowTraitProp>(
            """
            SELECT 
            	T.name trait_name, 
            	T.description trait_description, 
            	TP.id,
            	TP.trait_id,
            	TP.name,
            	TP.type,
            	COALESCE(ETP.value, TP.default_value) as value
            FROM public.mod_trait_properties TP
            INNER JOIN public.mod_trait T ON (T.id = TP.trait_id)
            INNER JOIN public.mod_element_trait ET ON ET.trait_id = T.id
            LEFT JOIN public.mod_element_trait_properties ETP ON ETP.trait_property_id = TP.id AND ET.id = ETP.element_trait_id
            WHERE ET.element_name = @elementName
            ORDER BY trait_name, TP.name
            """,
            new { elementName = elementTypeName }
        )).ToList();

        var traitMap = new Dictionary<string, ITrait>();
        
        foreach (var row in result)
        {
            var trait = MapTrait(row);
            var prop = MapTraitProperties(row);

            traitMap.TryAdd(row.trait_name, trait);
            traitMap[row.trait_name].Properties.TryAdd(row.name, prop);
        }

        return new TraitCollection(
            traitMap
        );
    }
    
    private ITrait MapTrait(DbRowTraitProp row)
    {
        return new Trait(
            new TraitId(row.trait_id),
            row.trait_name,
            row.trait_description,
            new Dictionary<string, IProperty>()
        );
    }

    private IProperty MapTraitProperties(DbRowTraitProp row)
    {
        return new Property(
            new TraitPropertyId(
                new TraitId(row.trait_id),
                row.id
            ),
            row.value == null ? new NullPropertyValue() : new PropertyValue(row.value)
        );
    }
    
    private struct DbRowTraitProp
    {
        public string trait_name { get; set; }
        public string trait_description { get; set; }
        public Guid id { get; set; }
        public Guid trait_id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string? value { get; set; }
    }
}