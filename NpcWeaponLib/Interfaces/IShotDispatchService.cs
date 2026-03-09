using NpcWeaponLib.Data;

namespace NpcWeaponLib.Interfaces;

/// <summary>
/// Dispatches a computed shot to the game server for impact processing.
/// </summary>
/// <remarks>
/// In the game backend, this maps to either:
/// <list type="bullet">
///   <item><c>ModManagerGrain.TriggerModAction()</c> with action ID 116 (custom shoot), or</item>
///   <item><c>INpcShotGrain.Fire()</c> (legacy direct path).</item>
/// </list>
/// The library computes <see cref="ShotData"/>; this service handles server-specific dispatch.
/// </remarks>
public interface IShotDispatchService
{
    /// <summary>
    /// Sends the computed shot to the game server for damage processing.
    /// </summary>
    /// <param name="shot">Complete shot data including weapon properties, positions, and ammo info.</param>
    /// <returns>A task that completes when the shot has been dispatched.</returns>
    Task DispatchShotAsync(ShotData shot);
}
