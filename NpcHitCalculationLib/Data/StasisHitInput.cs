namespace NpcHitCalculationLib.Data;

/// <summary>
/// Input parameters for determining whether a stasis weapon hits and computing effect strength.
/// </summary>
public class StasisHitInput
{
    /// <summary>Distance from weapon to target in metres.</summary>
    public required double Distance { get; set; }

    /// <summary>Effective range of the stasis weapon in metres.</summary>
    public required double Range { get; set; }

    /// <summary>Base effect strength of the stasis weapon (max-speed debuff magnitude).</summary>
    public required double BaseEffectStrength { get; set; }
}
