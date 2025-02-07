using System;
using System.Collections.Generic;
using MongoDB.Driver.Core.Misc;

namespace Mod.DynamicEncounters.Features.Loot.Data;

public class LootDefinitionItem
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<string> Tags { get; set; }
    public IEnumerable<LootItemRule> ItemRules { get; set; }
    public IEnumerable<ElementReplacementRule> ElementRules { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public required IEnumerable<string> ExtraTags { get; set; }

    public class ElementReplacementRule
    {
        public double Chance { get; set; } = 1;
        public long MinQuantity { get; set; } = 1;
        public long MaxQuantity { get; set; } = 1;
        public string FindElement { get; set; } = "";
        public string ReplaceElement { get; set; } = "";

        public bool IsValid() => !string.IsNullOrEmpty(FindElement) && !string.IsNullOrEmpty(ReplaceElement);
        
        public void Sanitize()
        {
            var quantityRange = SanitizeMinMax(MinQuantity, MaxQuantity);
            MinQuantity = quantityRange.Min;
            MaxQuantity = quantityRange.Max;

            Chance = Math.Clamp(Chance, 0, 1);
        }
        
        private Range<long> SanitizeMinMax(long min, long max)
        {
            if (min > max)
            {
                min = max;
            }

            if (max < min)
            {
                max = min;
            }

            return new Range<long>(min, max);
        }
    }
    
    public class LootItemRule(string itemName)
    {
        public string ItemName { get; set; } = itemName;
        public long MinQuantity { get; set; } = 1;
        public long MaxQuantity { get; set; } = 1;
        public long MinSpawnCost { get; set; } = 1;
        public long MaxSpawnCost { get; set; } = 1;
        public double Chance { get; set; } = 1;

        public void Sanitize()
        {
            var quantityRange = SanitizeMinMax(MinQuantity, MaxQuantity);
            MinQuantity = quantityRange.Min;
            MaxQuantity = quantityRange.Max;

            var spawnCostRange = SanitizeMinMax(MinSpawnCost, MaxSpawnCost);
            MinSpawnCost = spawnCostRange.Min;
            MaxSpawnCost = spawnCostRange.Max;

            Chance = Math.Clamp(Chance, 0, 1);
        }

        private Range<long> SanitizeMinMax(long min, long max)
        {
            if (min > max)
            {
                min = max;
            }

            if (max < min)
            {
                max = min;
            }

            return new Range<long>(min, max);
        }

        public LootItemRule CopyWithItemName(string name)
        {
            return new LootItemRule(name)
            {
                MinQuantity = MinQuantity,
                MaxQuantity = MaxQuantity,
                MinSpawnCost = MinSpawnCost,
                MaxSpawnCost = MaxSpawnCost,
                Chance = Chance
            };
        }

        public LootItemRule MultiplyBy(double value)
        {
            return new LootItemRule(ItemName)
            {
                MinQuantity = (long)(MinQuantity * value),
                MaxQuantity = (long)(MaxQuantity * value),
                MinSpawnCost = (long)(MinSpawnCost * 1 / value),
                MaxSpawnCost = (long)(MaxSpawnCost * 1 / value),
                Chance = Chance
            };
        }
        
        public LootItemRule MultiplyChanceBy(double value)
        {
            return new LootItemRule(ItemName)
            {
                MinQuantity = MinQuantity,
                MaxQuantity = MaxQuantity,
                MinSpawnCost = MinSpawnCost,
                MaxSpawnCost = MaxSpawnCost,
                Chance = Math.Clamp(Chance * value, 0, 1)
            };
        }
    }
}