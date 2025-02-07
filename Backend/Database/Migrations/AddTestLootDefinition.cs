using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using Mod.DynamicEncounters.Features.Loot.Data;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Database.Migrations;

[Migration(28)]
public class AddTestLootDefinition : Migration
{
    private const string LootTable = "mod_loot_def";

    public override void Up()
    {
        var ore1Types = new[] { "IronOre", "AluminiumOre", "CarbonOre", "SiliconOre" };
        var ore1LootRule = new LootDefinitionItem.LootItemRule("Ore1")
        {
            MinQuantity = 5000,
            MaxQuantity = 10000,
            MinSpawnCost = 250,
            MaxSpawnCost = 500
        };

        var ore1LootList = ore1Types
            .Select(o => ore1LootRule.CopyWithItemName(o));

        var ore2Types = new[] { "CalciumOre", "ChromiumOre", "CopperOre", "SodiumOre" };
        var ore2LootRule = new LootDefinitionItem.LootItemRule("Ore2")
        {
            MinQuantity = 5000,
            MaxQuantity = 7500,
            MinSpawnCost = 500,
            MaxSpawnCost = 750
        };

        var ore2LootList = ore2Types
            .Select(o => ore2LootRule.CopyWithItemName(o));

        var ore3Types = new[] { "LithiumOre", "NickelOre", "SilverOre", "SulfurOre" };
        var ore3LootRule = new LootDefinitionItem.LootItemRule("Ore3")
        {
            MinQuantity = 2500,
            MaxQuantity = 5000,
            MinSpawnCost = 750,
            MaxSpawnCost = 1000
        };

        var ore3LootList = ore3Types
            .Select(o => ore3LootRule.CopyWithItemName(o));

        var partSuffixes = new[] { 1, 2, 3 };
        var componentName = "component";
        var connectorName = "connector";
        var ledName = "led";
        var pipeName = "pipe";
        var screwName = "screw";

        var partLootRule = new LootDefinitionItem.LootItemRule("Ore1")
        {
            MinQuantity = 1000,
            MaxQuantity = 2000,
            MinSpawnCost = 200,
            MaxSpawnCost = 500
        };

        var componentTypes = partSuffixes.Select(x => new { name = $"{componentName}_{x}", tier = x })
            .Select(x => new { x.tier, loot = partLootRule.CopyWithItemName(x.name).MultiplyBy(1d / x.tier) })
            .ToArray();
        var connectorTypes = partSuffixes.Select(x => new { name = $"{connectorName}_{x}", tier = x })
            .Select(x => new { x.tier, loot = partLootRule.CopyWithItemName(x.name).MultiplyBy(1d / x.tier) })
            .ToArray();
        var ledTypes = partSuffixes.Select(x => new { name = $"{ledName}_{x}", tier = x })
            .Select(x => new { x.tier, loot = partLootRule.CopyWithItemName(x.name).MultiplyBy(1d / x.tier) })
            .ToArray();
        var pipeTypes = partSuffixes.Select(x => new { name = $"{pipeName}_{x}", tier = x })
            .Select(x => new { x.tier, loot = partLootRule.CopyWithItemName(x.name).MultiplyBy(1d / x.tier) })
            .ToArray();
        var screwTypes = partSuffixes.Select(x => new { name = $"{screwName}_{x}", tier = x })
            .Select(x => new { x.tier, loot = partLootRule.CopyWithItemName(x.name).MultiplyBy(1d / x.tier) })
            .ToArray();

        var ore12LootList = new List<LootDefinitionItem.LootItemRule>();
        ore12LootList.AddRange(ore1LootList);
        ore12LootList.AddRange(ore2LootList);

        var ore23LootList = new List<LootDefinitionItem.LootItemRule>();
        ore23LootList.AddRange(ore2LootList);
        ore23LootList.AddRange(ore3LootList);

        Insert.IntoTable(LootTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "Tier 1 and 2 Ores",
                tags = "[\"ore-1-2\"]",
                items = JsonConvert.SerializeObject(
                    ore12LootList
                )
            });

        Insert.IntoTable(LootTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "Tier 2 and 3 Ores",
                tags = "[\"ore-2-3\"]",
                items = JsonConvert.SerializeObject(
                    ore23LootList
                )
            });

        var parts12LootList = new List<LootDefinitionItem.LootItemRule>();
        parts12LootList.AddRange(screwTypes.Where(x => x.tier is 1 or 2).Select(x => x.loot));
        parts12LootList.AddRange(pipeTypes.Where(x => x.tier is 1 or 2).Select(x => x.loot));
        parts12LootList.AddRange(ledTypes.Where(x => x.tier is 1 or 2).Select(x => x.loot));
        parts12LootList.AddRange(connectorTypes.Where(x => x.tier is 1 or 2).Select(x => x.loot));
        parts12LootList.AddRange(componentTypes.Where(x => x.tier is 1 or 2).Select(x => x.loot));
        
        var parts23LootList = new List<LootDefinitionItem.LootItemRule>();
        parts23LootList.AddRange(screwTypes.Where(x => x.tier is 2 or 3).Select(x => x.loot));
        parts23LootList.AddRange(pipeTypes.Where(x => x.tier is 2 or 3).Select(x => x.loot));
        parts23LootList.AddRange(ledTypes.Where(x => x.tier is 2 or 3).Select(x => x.loot));
        parts23LootList.AddRange(connectorTypes.Where(x => x.tier is 2 or 3).Select(x => x.loot));
        parts23LootList.AddRange(componentTypes.Where(x => x.tier is 2 or 3).Select(x => x.loot));

        Insert.IntoTable(LootTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "Tier 1 and 2 Parts",
                tags = "[\"parts-1-2\"]",
                items = JsonConvert.SerializeObject(
                    parts12LootList
                )
            });
        
        Insert.IntoTable(LootTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "Tier 2 and 3 Parts",
                tags = "[\"parts-2-3\"]",
                items = JsonConvert.SerializeObject(
                    parts23LootList
                )
            });

        var functionalPartsList = new[]
        {
            "antenna_1_l",
            "antenna_1_m",
            "antenna_1_s",
            "antenna_1_xl",
            "antenna_1_xs",
            "antenna_2_l",
            "antenna_2_m",
            "antenna_2_s",
            "antenna_2_xl",
            "antenna_2_xs",
            "antenna_3_l",
            "antenna_3_m",
            "antenna_3_s",
            "antenna_3_xl",
            "antenna_3_xs",
            "chemicalcontainer_1_l",
            "chemicalcontainer_1_m",
            "chemicalcontainer_1_s",
            "chemicalcontainer_1_xl",
            "chemicalcontainer_1_xs",
            "chemicalcontainer_2_l",
            "chemicalcontainer_2_m",
            "chemicalcontainer_2_s",
            "chemicalcontainer_2_xl",
            "chemicalcontainer_2_xs",
            "chemicalcontainer_3_l",
            "chemicalcontainer_3_m",
            "chemicalcontainer_3_s",
            "chemicalcontainer_3_xl",
            "chemicalcontainer_3_xs",
            "combustionchamber_1_l",
            "combustionchamber_1_m",
            "combustionchamber_1_s",
            "combustionchamber_1_xl",
            "combustionchamber_1_xs",
            "combustionchamber_2_l",
            "combustionchamber_2_m",
            "combustionchamber_2_s",
            "combustionchamber_2_xl",
            "combustionchamber_2_xs",
            "combustionchamber_3_l",
            "combustionchamber_3_m",
            "combustionchamber_3_s",
            "combustionchamber_3_xl",
            "combustionchamber_3_xs",
            "controlsystem_1_l",
            "controlsystem_1_m",
            "controlsystem_1_s",
            "controlsystem_1_xl",
            "controlsystem_1_xs",
            "controlsystem_2_l",
            "controlsystem_2_m",
            "controlsystem_2_s",
            "controlsystem_2_xl",
            "controlsystem_2_xs",
            "controlsystem_3_l",
            "controlsystem_3_m",
            "controlsystem_3_s",
            "controlsystem_3_xl",
            "controlsystem_3_xs",
            "coresystem_1_l",
            "coresystem_1_m",
            "coresystem_1_s",
            "coresystem_1_xl",
            "coresystem_1_xs",
            "coresystem_2_l",
            "coresystem_2_m",
            "coresystem_2_s",
            "coresystem_2_xl",
            "coresystem_2_xs",
            "coresystem_3_l",
            "coresystem_3_m",
            "coresystem_3_s",
            "coresystem_3_xl",
            "coresystem_3_xs",
            "electricengine_1_l",
            "electricengine_1_m",
            "electricengine_1_s",
            "electricengine_1_xl",
            "electricengine_1_xs",
            "electricengine_2_l",
            "electricengine_2_m",
            "electricengine_2_s",
            "electricengine_2_xl",
            "electricengine_2_xs",
            "electricengine_3_l",
            "electricengine_3_m",
            "electricengine_3_s",
            "electricengine_3_xl",
            "electricengine_3_xs",
            "firingsystem_1_l",
            "firingsystem_1_m",
            "firingsystem_1_s",
            "firingsystem_1_xl",
            "firingsystem_1_xs",
            "firingsystem_2_l",
            "firingsystem_2_m",
            "firingsystem_2_s",
            "firingsystem_2_xl",
            "firingsystem_2_xs",
            "firingsystem_3_l",
            "firingsystem_3_m",
            "firingsystem_3_s",
            "firingsystem_3_xl",
            "firingsystem_3_xs",
            "gazcylinder_1_l",
            "gazcylinder_1_m",
            "gazcylinder_1_s",
            "gazcylinder_1_xl",
            "gazcylinder_1_xs",
            "gazcylinder_2_l",
            "gazcylinder_2_m",
            "gazcylinder_2_s",
            "gazcylinder_2_xl",
            "gazcylinder_2_xs",
            "gazcylinder_3_l",
            "gazcylinder_3_m",
            "gazcylinder_3_s",
            "gazcylinder_3_xl",
            "gazcylinder_3_xs",
            "ionicchamber_1_l",
            "ionicchamber_1_m",
            "ionicchamber_1_s",
            "ionicchamber_1_xl",
            "ionicchamber_1_xs",
            "ionicchamber_2_l",
            "ionicchamber_2_m",
            "ionicchamber_2_s",
            "ionicchamber_2_xl",
            "ionicchamber_2_xs",
            "ionicchamber_3_l",
            "ionicchamber_3_m",
            "ionicchamber_3_s",
            "ionicchamber_3_xl",
            "ionicchamber_3_xs",
            "laserchamber_1_l",
            "laserchamber_1_m",
            "laserchamber_1_s",
            "laserchamber_1_xl",
            "laserchamber_1_xs",
            "laserchamber_2_l",
            "laserchamber_2_m",
            "laserchamber_2_s",
            "laserchamber_2_xl",
            "laserchamber_2_xs",
            "laserchamber_3_l",
            "laserchamber_3_m",
            "laserchamber_3_s",
            "laserchamber_3_xl",
            "laserchamber_3_xs",
            "light_1_l",
            "light_1_m",
            "light_1_s",
            "light_1_xl",
            "light_1_xs",
            "light_2_l",
            "light_2_m",
            "light_2_s",
            "light_2_xl",
            "light_2_xs",
            "light_3_l",
            "light_3_m",
            "light_3_s",
            "light_3_xl",
            "light_3_xs",
            "magneticrail_1_l",
            "magneticrail_1_m",
            "magneticrail_1_s",
            "magneticrail_1_xl",
            "magneticrail_1_xs",
            "magneticrail_2_l",
            "magneticrail_2_m",
            "magneticrail_2_s",
            "magneticrail_2_xl",
            "magneticrail_2_xs",
            "magneticrail_3_l",
            "magneticrail_3_m",
            "magneticrail_3_s",
            "magneticrail_3_xl",
            "magneticrail_3_xs",
            "mechanicalsensor_1_l",
            "mechanicalsensor_1_m",
            "mechanicalsensor_1_s",
            "mechanicalsensor_1_xl",
            "mechanicalsensor_1_xs",
            "mechanicalsensor_2_l",
            "mechanicalsensor_2_m",
            "mechanicalsensor_2_s",
            "mechanicalsensor_2_xl",
            "mechanicalsensor_2_xs",
            "mechanicalsensor_3_l",
            "mechanicalsensor_3_m",
            "mechanicalsensor_3_s",
            "mechanicalsensor_3_xl",
            "mechanicalsensor_3_xs",
            "mobilepanel_1_l",
            "mobilepanel_1_m",
            "mobilepanel_1_s",
            "mobilepanel_1_xl",
            "mobilepanel_1_xs",
            "mobilepanel_2_l",
            "mobilepanel_2_m",
            "mobilepanel_2_s",
            "mobilepanel_2_xl",
            "mobilepanel_2_xs",
            "mobilepanel_3_l",
            "mobilepanel_3_m",
            "mobilepanel_3_s",
            "mobilepanel_3_xl",
            "mobilepanel_3_xs",
            "motherboard_1_l",
            "motherboard_1_m",
            "motherboard_1_s",
            "motherboard_1_xl",
            "motherboard_1_xs",
            "motherboard_2_l",
            "motherboard_2_m",
            "motherboard_2_s",
            "motherboard_2_xl",
            "motherboard_2_xs",
            "motherboard_3_l",
            "motherboard_3_m",
            "motherboard_3_s",
            "motherboard_3_xl",
            "motherboard_3_xs",
            "opticalsensor_1_l",
            "opticalsensor_1_m",
            "opticalsensor_1_s",
            "opticalsensor_1_xl",
            "opticalsensor_1_xs",
            "opticalsensor_2_l",
            "opticalsensor_2_m",
            "opticalsensor_2_s",
            "opticalsensor_2_xl",
            "opticalsensor_2_xs",
            "opticalsensor_3_l",
            "opticalsensor_3_m",
            "opticalsensor_3_s",
            "opticalsensor_3_xl",
            "opticalsensor_3_xs",
            "orescanner_1_l",
            "orescanner_1_m",
            "orescanner_1_s",
            "orescanner_1_xl",
            "orescanner_1_xs",
            "orescanner_2_l",
            "orescanner_2_m",
            "orescanner_2_s",
            "orescanner_2_xl",
            "orescanner_2_xs",
            "orescanner_3_l",
            "orescanner_3_m",
            "orescanner_3_s",
            "orescanner_3_xl",
            "orescanner_3_xs",
            "powerconvertor_1_l",
            "powerconvertor_1_m",
            "powerconvertor_1_s",
            "powerconvertor_1_xl",
            "powerconvertor_1_xs",
            "powerconvertor_2_l",
            "powerconvertor_2_m",
            "powerconvertor_2_s",
            "powerconvertor_2_xl",
            "powerconvertor_2_xs",
            "powerconvertor_3_l",
            "powerconvertor_3_m",
            "powerconvertor_3_s",
            "powerconvertor_3_xl",
            "powerconvertor_3_xs",
            "roboticarm_1_l",
            "roboticarm_1_m",
            "roboticarm_1_s",
            "roboticarm_1_xl",
            "roboticarm_1_xs",
            "roboticarm_2_l",
            "roboticarm_2_m",
            "roboticarm_2_s",
            "roboticarm_2_xl",
            "roboticarm_2_xs",
            "roboticarm_3_l",
            "roboticarm_3_m",
            "roboticarm_3_s",
            "roboticarm_3_xl",
            "roboticarm_3_xs",
            "screen_1_l",
            "screen_1_m",
            "screen_1_s",
            "screen_1_xl",
            "screen_1_xs",
            "screen_2_l",
            "screen_2_m",
            "screen_2_s",
            "screen_2_xl",
            "screen_2_xs",
            "screen_3_l",
            "screen_3_m",
            "screen_3_s",
            "screen_3_xl",
            "screen_3_xs",
            "silo_1_l",
            "silo_1_m",
            "silo_1_s",
            "silo_1_xl",
            "silo_1_xs",
            "silo_2_l",
            "silo_2_m",
            "silo_2_s",
            "silo_2_xl",
            "silo_2_xs",
            "silo_3_l",
            "silo_3_m",
            "silo_3_s",
            "silo_3_xl",
            "silo_3_xs",
        };

        var tieredFunctionalParts = functionalPartsList.Select(p =>
        {
            var split = p.Split("_");
            var tier = split[1];

            return new
            {
                name = p,
                tier = int.Parse(tier)
            };
        }).ToList();

        var functionalPartLootRule = new LootDefinitionItem.LootItemRule("")
        {
            MinQuantity = 1,
            MaxQuantity = 200,
            MinSpawnCost = 1,
            MaxSpawnCost = 200,
            Chance = 0.80d
        };

        var functionalParts12LootRules = tieredFunctionalParts
            .Where(p => p.tier is 1 or 2)
            .Select(p => functionalPartLootRule
                .CopyWithItemName(p.name)
                .MultiplyBy(1d / p.tier)
                .MultiplyChanceBy(1d / p.tier)
            );

        var functionalParts23LootRules = tieredFunctionalParts
            .Where(p => p.tier is 2 or 3)
            .Select(p => functionalPartLootRule
                .CopyWithItemName(p.name)
                .MultiplyBy(1d / p.tier)
                .MultiplyChanceBy(1d / p.tier)
            );

        Insert.IntoTable(LootTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "Tier 1 to 2 Parts",
                tags = "[\"functional-parts-1-2\"]",
                items = JsonConvert.SerializeObject(
                    functionalParts12LootRules
                )
            });

        Insert.IntoTable(LootTable)
            .InSchema("public")
            .Row(new
            {
                id = Guid.NewGuid(),
                name = "Tier 2 to 3 Parts",
                tags = "[\"functional-parts-2-3\"]",
                items = JsonConvert.SerializeObject(
                    functionalParts23LootRules
                )
            });
    }

    public override void Down()
    {
    }
}