using NpcCommonLib.Data;

namespace NpcTargetingLib.Strategies;

/// <summary>
/// Selects the nearest radar contact by distance.
/// </summary>
/// <remarks>
/// Ported from <c>ClosestSelectRadarTargetEffect</c>. Stateless, pure function.
/// </remarks>
public class ClosestTargetStrategy : ITargetSelectionStrategy
{
    public ScanContact? SelectTarget(TargetSelectionParams @params)
    {
        return @params.Contacts.Count == 0 ? null : @params.Contacts.MinBy(c => c.Distance);
    }
}
