using NpcCommonLib.Data;

namespace NpcTargetingLib.Strategies;

/// <summary>
/// Selects the contact that dealt the most damage recently.
/// Falls back to closest contact if no recent damage.
/// </summary>
/// <remarks>
/// Ported from <c>HighestThreatRadarTargetEffect</c>.
/// This is the <b>default</b> targeting strategy in the original game
/// (registered in <c>EffectHandler</c>).
/// </remarks>
public class HighestThreatTargetStrategy : ITargetSelectionStrategy
{
    /// <inheritdoc/>
    public ScanContact? SelectTarget(TargetSelectionParams @params)
    {
        var threatId = ThreatCalculator.GetHighestThreat(
            @params.DamageHistory, @params.Contacts);

        if (threatId == null) return null;

        // Return the contact matching the threat — it must still be on radar
        return @params.Contacts.FirstOrDefault(c => c.ConstructId == threatId.Value);
    }
}
