namespace Mod.DynamicEncounters.Tests;

public class Tests
{
    private readonly List<FactionEntity> _factions =
    [
        new FactionEntity
        {
            FactionEntityId = new FactionEntityId("red-talon"),
            DiplomacyStance = new Dictionary<FactionEntityId, FactionStance>
            {
                { new FactionEntityId("pirates"), FactionStance.War },
                { new FactionEntityId("uef"), FactionStance.Peace },
            },
            Territories = [
                new SpaceStation(new TerritoryId("feli-market"))
            ]
        },
        new FactionEntity
        {
            FactionEntityId = new FactionEntityId("uef"),
            DiplomacyStance = new Dictionary<FactionEntityId, FactionStance>
            {
                { new FactionEntityId("pirates"), FactionStance.War },
                { new FactionEntityId("red-talon"), FactionStance.Peace },
            },
            Territories = [
                new SpaceStation(new TerritoryId("alioth-market-6"))
            ]
        },
        new FactionEntity
        {
            FactionEntityId = new FactionEntityId("pirates"),
            DiplomacyStance = new Dictionary<FactionEntityId, FactionStance>
            {
                { new FactionEntityId("uef"), FactionStance.War },
                { new FactionEntityId("red-talon"), FactionStance.War },
            },
            Territories = [
                new SpaceStation(new TerritoryId("port-royal"))
            ]
        }
    ];

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
}

public readonly struct FactionEntityId(string id)
{
    public string Id { get; init; } = id;
}

public readonly struct TerritoryId(string id)
{
    public string Id { get; init; } = id;
}

public class FactionEntity
{
    public FactionEntityId FactionEntityId { get; set; }
    public Dictionary<FactionEntityId, FactionStance> DiplomacyStance { get; set; } = [];
    public IEnumerable<IFactionTerritory> Territories { get; set; } = [];
}

public enum TerritoryType
{
    CommercialHub,
    MiningOutpost,
    Industrial,
    Military
}

public enum FactionStance
{
    Peace,
    War
}

public enum FactionState
{
    Expanding,
    AtWar,
    Stable
}

public interface IFactionTerritory
{
    TerritoryId TerritoryId { get; set; }
    IEnumerable<IItemImport> Imports { get; set; }
    IEnumerable<IItemExport> Exports { get; set; }
}

public class PlanetaryOutpost(TerritoryId territoryId) : IFactionTerritory
{
    public TerritoryId TerritoryId { get; set; } = territoryId;
    public IEnumerable<IItemImport> Imports { get; set; } = [];
    public IEnumerable<IItemExport> Exports { get; set; } = [];
}

public class SpaceStation(TerritoryId territoryId) : IFactionTerritory
{
    public TerritoryId TerritoryId { get; set; } = territoryId;
    public IEnumerable<IItemImport> Imports { get; set; } = [];
    public IEnumerable<IItemExport> Exports { get; set; } = [];
}

public class TerritoryModifiers
{
    public long Security { get; set; }
    public long Economy { get; set; }
}

public interface IItemImport
{
    ItemId ItemId { get; set; }
    double DemandScore { get; set; } 
}

public interface IItemExport
{
    ItemId ItemId { get; set; }
    double ProductionRatio { get; set; }
}

public readonly struct ItemId(string id)
{
    public string Id { get; } = id;
}