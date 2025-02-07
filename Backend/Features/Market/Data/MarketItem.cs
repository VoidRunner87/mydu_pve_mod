namespace Mod.DynamicEncounters.Features.Market.Data;

public class MarketItem
{
    public long Quantity { get; set; }
    public long Price { get; set; }
    public ulong ItemTypeId { get; set; }
    public ulong MarketId { get; set; }
    public ulong OwnerId { get; set; }
}