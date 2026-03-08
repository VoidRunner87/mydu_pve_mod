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
    Task DispatchShotAsync(ShotData shot);
}
