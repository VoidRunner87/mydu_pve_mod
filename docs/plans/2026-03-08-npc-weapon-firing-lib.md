# NPC Weapon Firing Library Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Extract NPC weapon firing logic into a standalone, pure data-in/data-out library (`NpcWeaponLib`) that follows the same isolation pattern as `NpcMovementLib`.

**Architecture:** Pure C# class library with zero game-server dependencies. All weapon stats, fire rate calculations, weapon selection, and shot timing are pure functions. Game-server interactions (hit position queries, shot dispatch, safe zone checks, weapon health reads) are abstracted behind interfaces that consumers implement. The library computes *what* to fire and *when* — the consumer handles *how* to send it to the game.

**Tech Stack:** C# / .NET 8.0, no external NuGet dependencies (BCL only). Reuses `NpcMovementLib.Math.Vec3` and `NpcMovementLib.Data.ConstructId` via project reference.

---

## Project Structure

```
NpcWeaponLib/
├── NpcWeaponLib.csproj                    (net8.0, refs NpcMovementLib only)
├── Data/
│   ├── WeaponStats.cs                     (weapon properties — pure data, no game SDK)
│   ├── AmmoStats.cs                       (ammo properties — damage type, volume, tier)
│   ├── WeaponModifiers.cs                 (multipliers for damage, accuracy, cycle time, etc.)
│   ├── WeaponEffectiveness.cs             (per-weapon health status)
│   ├── FiringInput.cs                     (all inputs for a single fire tick)
│   ├── FiringOutput.cs                    (result: should fire, weapon config, timing)
│   └── ShotData.cs                        (complete shot context for server dispatch)
├── WeaponFireRateCalculator.cs            (pure: magazine, cycle time, reload → fire rate)
├── WeaponSelector.cs                      (pure: pick best weapon by target distance)
├── FiringSimulator.cs                     (orchestrator: accumulator + selection + output)
├── Interfaces/
│   ├── IWeaponHealthService.cs            (read weapon hitpoint ratios from game)
│   ├── IShotDispatchService.cs            (send shot to game server)
│   ├── IHitPositionService.cs             (query voxel service for hit point on target)
│   └── ISafeZoneService.cs                (check if construct is in safe zone)
└── README.md
```

---

## Task 1: Create project and data types

**Files:**
- Create: `NpcWeaponLib/NpcWeaponLib.csproj`
- Create: `NpcWeaponLib/Data/AmmoStats.cs`
- Create: `NpcWeaponLib/Data/WeaponStats.cs`
- Create: `NpcWeaponLib/Data/WeaponModifiers.cs`
- Create: `NpcWeaponLib/Data/WeaponEffectiveness.cs`

### Step 1: Create the project file

```xml
<!-- NpcWeaponLib/NpcWeaponLib.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\NpcMovementLib\NpcMovementLib.csproj" />
  </ItemGroup>
</Project>
```

### Step 2: Create AmmoStats

Port from `Backend/Features/Common/Data/AmmoItem.cs`. Drop the `NQutils.Def.Ammo` constructor dependency.

```csharp
namespace NpcWeaponLib.Data;

/// <summary>
/// Ammunition properties for a weapon system. Pure data — no game SDK dependencies.
/// </summary>
/// <remarks>
/// Ported from <c>Mod.DynamicEncounters.Features.Common.Data.AmmoItem</c>.
/// The original class wraps <c>NQutils.Def.Ammo</c>; this version is standalone.
/// </remarks>
public class AmmoStats
{
    /// <summary>Unique item type identifier from the game bank.</summary>
    public required ulong ItemTypeId { get; set; }

    /// <summary>Internal type name (e.g., "AmmoCannonSmallKineticAdvancedAgile").</summary>
    public required string ItemTypeName { get; set; }

    /// <summary>Weapon scale category (e.g., "xs", "s", "m", "l").</summary>
    public required string Scale { get; set; }

    /// <summary>Ammo tier level (1-5). Used with <see cref="FiringInput.AmmoTier"/> to filter compatible ammo.</summary>
    public required int Level { get; set; }

    /// <summary>
    /// Damage type identifier (antimatter, electromagnetic, kinetic, thermic).
    /// Stored as string to avoid dependency on <c>NQutils.Def.DamageType</c> enum.
    /// </summary>
    public required string DamageType { get; set; }

    /// <summary>
    /// Volume of a single ammo unit in litres.
    /// Used to calculate magazine capacity: <c>Floor(MagazineVolume * magBuff / UnitVolume)</c>.
    /// </summary>
    public required double UnitVolume { get; set; }
}
```

### Step 3: Create WeaponStats

Port from `Backend/Features/Common/Data/WeaponItem.cs`. Keep the fire rate calculation methods as pure functions — these are the core of the weapon system.

```csharp
namespace NpcWeaponLib.Data;

/// <summary>
/// Complete weapon statistics and fire rate calculations. Pure data with pure methods.
/// </summary>
/// <remarks>
/// <para>
/// Ported from <c>Mod.DynamicEncounters.Features.Common.Data.WeaponItem</c>.
/// The original wraps <c>NQutils.Def.WeaponUnit</c>; this version is standalone.
/// </para>
/// <para>
/// Fire rate pipeline:
/// <list type="number">
///   <item><see cref="GetNumberOfShotsInMagazine"/> — magazine volume / ammo volume</item>
///   <item><see cref="GetTimeToEmpty"/> — shots * cycle time</item>
///   <item><see cref="GetTotalCycleTime"/> — time to empty + reload time</item>
///   <item><see cref="GetSustainedRateOfFire"/> — shots / total cycle</item>
///   <item><see cref="GetShotWaitTime"/> — 1 / sustained ROF (clamped)</item>
///   <item><see cref="GetShotWaitTimePerGun"/> — wait time / weapon count</item>
/// </list>
/// </para>
/// </remarks>
public class WeaponStats
{
    /// <summary>Default magazine buff factor applied to magazine volume (1.5x).</summary>
    public const double FullMagBuff = 1.5d;

    /// <summary>Default buff factor applied to cycle time and reload time (0.5625x).</summary>
    public const double FullBuff = 0.5625d;

    public required ulong ItemTypeId { get; set; }
    public required string ItemTypeName { get; set; }
    public required string DisplayName { get; set; }

    /// <summary>Base damage per shot before modifiers.</summary>
    public required double BaseDamage { get; set; }

    /// <summary>Base hit probability at optimal range (0-1).</summary>
    public required double BaseAccuracy { get; set; }

    /// <summary>Optimal engagement distance in metres.</summary>
    public required double BaseOptimalDistance { get; set; }

    /// <summary>Distance beyond optimal where effectiveness degrades, in metres.</summary>
    public required double FalloffDistance { get; set; }

    /// <summary>Tracking effectiveness at optimal range.</summary>
    public required double BaseOptimalTracking { get; set; }

    /// <summary>Tracking degradation beyond optimal range.</summary>
    public required double FalloffTracking { get; set; }

    /// <summary>Aiming cone half-angle at optimal range.</summary>
    public required double BaseOptimalAimingCone { get; set; }

    /// <summary>Aiming cone expansion beyond optimal range.</summary>
    public required double FalloffAimingCone { get; set; }

    /// <summary>Ideal target cross-section diameter in metres.</summary>
    public required double OptimalCrossSectionDiameter { get; set; }

    /// <summary>Time between shots in a single magazine, in seconds.</summary>
    public required double BaseCycleTime { get; set; }

    /// <summary>Time to reload an empty magazine, in seconds.</summary>
    public required double BaseReloadTime { get; set; }

    /// <summary>Magazine capacity in litres (before buff factor).</summary>
    public required double MagazineVolume { get; set; }

    /// <summary>All compatible ammo types for this weapon.</summary>
    public required IReadOnlyList<AmmoStats> AmmoItems { get; set; }

    /// <summary>
    /// Half-falloff firing distance: optimal + falloff/2.
    /// Used by <see cref="WeaponSelector"/> to match weapons to target range.
    /// </summary>
    public double GetHalfFalloffDistance() => BaseOptimalDistance + FalloffDistance / 2;

    /// <summary>Number of shots per magazine given ammo unit volume and magazine buff.</summary>
    public double GetNumberOfShotsInMagazine(AmmoStats ammo, double magBuff = FullMagBuff)
        => Math.Floor(MagazineVolume * magBuff / ammo.UnitVolume);

    /// <summary>Time to fire all shots in a full magazine, in seconds.</summary>
    public double GetTimeToEmpty(AmmoStats ammo, double magBuff = FullMagBuff, double cycleBuff = FullBuff)
        => GetNumberOfShotsInMagazine(ammo, magBuff) * (BaseCycleTime * cycleBuff);

    /// <summary>Reload duration in seconds, with buff factor.</summary>
    public double GetReloadTime(double reloadBuff = FullBuff) => BaseReloadTime * reloadBuff;

    /// <summary>Full cycle: fire all shots + reload, in seconds.</summary>
    public double GetTotalCycleTime(AmmoStats ammo, double magBuff = FullMagBuff, double cycleBuff = FullBuff, double reloadBuff = FullBuff)
        => GetTimeToEmpty(ammo, magBuff, cycleBuff) + GetReloadTime(reloadBuff);

    /// <summary>Average shots per second across full cycle (fire + reload).</summary>
    public double GetSustainedRateOfFire(AmmoStats ammo, double magBuff = FullMagBuff, double cycleBuff = FullBuff, double reloadBuff = FullBuff)
        => GetNumberOfShotsInMagazine(ammo, magBuff) / GetTotalCycleTime(ammo, magBuff, cycleBuff, reloadBuff);

    /// <summary>
    /// Seconds between shots for a single weapon, accounting for sustained ROF.
    /// Clamped: buff factors to [0.1, 5], result floor at BaseCycleTime if ROF is too high.
    /// </summary>
    public double GetShotWaitTime(AmmoStats ammo, double magBuff = FullMagBuff, double cycleBuff = FullBuff, double reloadBuff = FullBuff)
    {
        cycleBuff = Math.Clamp(cycleBuff, 0.1d, 5d);
        reloadBuff = Math.Clamp(reloadBuff, 0.1d, 5d);
        magBuff = Math.Clamp(magBuff, 0.1d, 5d);

        var result = 1d / GetSustainedRateOfFire(ammo, magBuff, cycleBuff, reloadBuff);
        return result <= 0.5d ? Math.Clamp(BaseCycleTime, 0.5d, 60d) : result;
    }

    /// <summary>
    /// Effective wait time when N guns of this type fire in parallel.
    /// <paramref name="weaponCount"/> is clamped to [1, 10].
    /// </summary>
    public double GetShotWaitTimePerGun(AmmoStats ammo, int weaponCount, double magBuff = FullMagBuff, double cycleBuff = FullBuff, double reloadBuff = FullBuff)
        => GetShotWaitTime(ammo, magBuff, cycleBuff, reloadBuff) / Math.Clamp(weaponCount, 1d, 10d);
}
```

### Step 4: Create WeaponModifiers

Port from `Backend/Features/Spawner/Data/BehaviorModifiers.WeaponModifiers`.

```csharp
namespace NpcWeaponLib.Data;

/// <summary>
/// Multipliers applied to base weapon stats. All default to 1.0 (no modification).
/// </summary>
/// <remarks>
/// Ported from <c>BehaviorModifiers.WeaponModifiers</c>. These are set per-NPC prefab
/// to create weapon variants (e.g., 2x damage boss, 0.5x cycle time rapid-fire ship).
/// Each modifier is multiplied against the corresponding <see cref="WeaponStats"/> base value
/// when constructing the final shot parameters.
/// </remarks>
public class WeaponModifiers
{
    public float Damage { get; set; } = 1;
    public float Accuracy { get; set; } = 1;
    public float CycleTime { get; set; } = 1;
    public float OptimalDistance { get; set; } = 1;
    public float FalloffDistance { get; set; } = 1;
    public float FalloffAimingCone { get; set; } = 1;
    public float FalloffTracking { get; set; } = 1;
    public float OptimalTracking { get; set; } = 1;
    public float OptimalAimingCone { get; set; } = 1;
}
```

### Step 5: Create WeaponEffectiveness

Port from `Backend/Features/Common/Data/WeaponEffectivenessData`.

```csharp
namespace NpcWeaponLib.Data;

/// <summary>
/// Health status of an individual weapon element on a construct.
/// </summary>
/// <remarks>
/// Weapons with <see cref="HitPointsRatio"/> at or below 1% are considered destroyed
/// and excluded from firing. The NPC selects only functional weapons.
/// </remarks>
public class WeaponEffectiveness
{
    /// <summary>Internal item type name matching <see cref="WeaponStats.ItemTypeName"/>.</summary>
    public required string Name { get; set; }

    /// <summary>Current hitpoints as a ratio of max (0.0 = destroyed, 1.0 = full health).</summary>
    public required double HitPointsRatio { get; set; }

    /// <summary>Returns true if hitpoints are at or below 1% — weapon is non-functional.</summary>
    public bool IsDestroyed() => HitPointsRatio <= 0.01d;
}
```

### Step 6: Build to verify

Run: `dotnet build NpcWeaponLib/NpcWeaponLib.csproj`
Expected: Build succeeded, 0 warnings, 0 errors

### Step 7: Commit

```bash
git add NpcWeaponLib/
git commit -m "feat: add NpcWeaponLib project with weapon data types"
```

---

## Task 2: Weapon selection and fire rate calculator

**Files:**
- Create: `NpcWeaponLib/WeaponSelector.cs`
- Create: `NpcWeaponLib/WeaponFireRateCalculator.cs`

### Step 1: Create WeaponSelector

Port weapon selection logic from `BehaviorContext.GetBestFunctionalWeaponByTargetDistance()` and `ConstructDamageData`. Pure static functions.

```csharp
namespace NpcWeaponLib;

using NpcWeaponLib.Data;

/// <summary>
/// Selects the best weapon for engagement based on target distance and weapon health.
/// All methods are pure functions with no side effects.
/// </summary>
/// <remarks>
/// Ported from <c>ConstructDamageData.GetBestWeaponByTargetDistance()</c> and
/// <c>BehaviorContext.GetBestFunctionalWeaponByTargetDistance()</c>.
///
/// Selection algorithm:
/// <list type="number">
///   <item>Filter to only functional weapons (hitpoints &gt; 1%).</item>
///   <item>For each functional weapon, compute half-falloff distance (optimal + falloff/2).</item>
///   <item>Select the weapon whose half-falloff distance is closest to target distance.</item>
/// </list>
/// </remarks>
public static class WeaponSelector
{
    /// <summary>
    /// Picks the best functional weapon for the given target distance.
    /// Returns null if no functional weapons remain.
    /// </summary>
    /// <param name="weapons">All weapons on the construct.</param>
    /// <param name="effectiveness">Per-weapon health data, keyed by ItemTypeName.</param>
    /// <param name="targetDistance">Distance to target in metres.</param>
    public static WeaponStats? SelectBestWeapon(
        IEnumerable<WeaponStats> weapons,
        IDictionary<string, IList<WeaponEffectiveness>> effectiveness,
        double targetDistance)
    {
        var functionalNames = effectiveness
            .SelectMany(kvp => kvp.Value)
            .Where(e => !e.IsDestroyed())
            .Select(e => e.Name)
            .ToHashSet();

        var functionalWeapons = weapons.Where(w => functionalNames.Contains(w.ItemTypeName));

        return functionalWeapons
            .Select(w => new { Weapon = w, Delta = Math.Abs(w.GetHalfFalloffDistance() - targetDistance) })
            .MinBy(x => x.Delta)?.Weapon;
    }

    /// <summary>
    /// Returns (functionalCount, totalCount) for a specific weapon type.
    /// </summary>
    public static (int FunctionalCount, int TotalCount) GetEffectivenessFactors(
        IDictionary<string, IList<WeaponEffectiveness>> effectiveness,
        string itemTypeName)
    {
        if (!effectiveness.TryGetValue(itemTypeName, out var list) || list.Count == 0)
            return (0, 1);

        return (list.Count(x => !x.IsDestroyed()), list.Count);
    }

    /// <summary>Returns true if any weapon across all types is still functional.</summary>
    public static bool HasAnyFunctionalWeapons(IDictionary<string, IList<WeaponEffectiveness>> effectiveness)
    {
        return effectiveness.SelectMany(kvp => kvp.Value).Any(e => !e.IsDestroyed());
    }
}
```

### Step 2: Create WeaponFireRateCalculator

A convenience static class that wraps the fire rate pipeline into a single call, given weapon + ammo + modifiers + weapon count. This is the "what's my fire interval?" question answered in one call.

```csharp
namespace NpcWeaponLib;

using NpcWeaponLib.Data;

/// <summary>
/// Computes the effective fire interval for a weapon given ammo, modifiers, and weapon count.
/// Pure function — wraps <see cref="WeaponStats"/> fire rate methods with modifier application.
/// </summary>
public static class WeaponFireRateCalculator
{
    /// <summary>
    /// Calculates the seconds between shots for the given weapon configuration.
    /// </summary>
    /// <param name="weapon">Weapon base stats.</param>
    /// <param name="ammo">Selected ammo type.</param>
    /// <param name="modifiers">Per-NPC weapon modifiers.</param>
    /// <param name="functionalWeaponCount">Number of functional weapons of this type (clamped to [1,10]).</param>
    /// <param name="maxWeaponCount">Max weapon count from prefab config (caps functionalWeaponCount).</param>
    /// <returns>Seconds between shots. Lower = faster firing.</returns>
    public static double CalculateFireInterval(
        WeaponStats weapon,
        AmmoStats ammo,
        WeaponModifiers modifiers,
        int functionalWeaponCount,
        int maxWeaponCount)
    {
        var clampedCount = Math.Clamp(functionalWeaponCount, 0, maxWeaponCount);
        return weapon.GetShotWaitTimePerGun(ammo, clampedCount, cycleTimeBuffFactor: modifiers.CycleTime);
    }
}
```

### Step 3: Build and commit

Run: `dotnet build NpcWeaponLib/NpcWeaponLib.csproj`

```bash
git add NpcWeaponLib/WeaponSelector.cs NpcWeaponLib/WeaponFireRateCalculator.cs
git commit -m "feat: add weapon selection and fire rate calculator"
```

---

## Task 3: Firing input/output and shot data

**Files:**
- Create: `NpcWeaponLib/Data/FiringInput.cs`
- Create: `NpcWeaponLib/Data/FiringOutput.cs`
- Create: `NpcWeaponLib/Data/ShotData.cs`

### Step 1: Create FiringInput

Flat input struct containing everything a single fire tick needs — analogous to `MovementInput`.

```csharp
using NpcMovementLib.Data;
using NpcMovementLib.Math;

namespace NpcWeaponLib.Data;

/// <summary>
/// All inputs needed for a single weapon firing tick.
/// Analogous to <see cref="NpcMovementLib.Data.MovementInput"/> for the movement system.
/// </summary>
public class FiringInput
{
    // --- NPC State ---

    /// <summary>NPC construct identifier.</summary>
    public required ConstructId ConstructId { get; set; }

    /// <summary>NPC's current world-space position in metres.</summary>
    public required Vec3 Position { get; set; }

    /// <summary>NPC construct's bounding size (used for hit position fallback).</summary>
    public required ulong ConstructSize { get; set; }

    /// <summary>Whether the NPC is alive. If false, firing is suppressed.</summary>
    public required bool IsAlive { get; set; }

    // --- Target State ---

    /// <summary>Target construct identifier. Null or 0 = no target, skip firing.</summary>
    public ConstructId? TargetConstructId { get; set; }

    /// <summary>Target's world-space position in metres.</summary>
    public Vec3 TargetPosition { get; set; }

    // --- Weapons ---

    /// <summary>All weapons on this NPC construct.</summary>
    public required IReadOnlyList<WeaponStats> Weapons { get; set; }

    /// <summary>Per-weapon-type health data, keyed by ItemTypeName.</summary>
    public required IDictionary<string, IList<WeaponEffectiveness>> WeaponEffectiveness { get; set; }

    /// <summary>Weapon stat multipliers from the NPC prefab.</summary>
    public required WeaponModifiers Modifiers { get; set; }

    // --- Ammo Config ---

    /// <summary>Required ammo tier level (1-5). Filters compatible ammo from weapon's ammo list.</summary>
    public required int AmmoTier { get; set; }

    /// <summary>
    /// Ammo variant name filter (case-insensitive contains match).
    /// E.g., "Kinetic", "Thermic". Filters compatible ammo from weapon's ammo list.
    /// </summary>
    public required string AmmoVariant { get; set; }

    // --- Timing ---

    /// <summary>Seconds since last tick. Accumulated until fire interval is reached.</summary>
    public required double DeltaTime { get; set; }

    /// <summary>Max weapon count from prefab config. Caps the functional weapon count.</summary>
    public required int MaxWeaponCount { get; set; }

    // --- Max Engagement Range ---

    /// <summary>
    /// Maximum engagement distance in metres. Shots beyond this range are suppressed.
    /// Default: 2 SU (400,000 m) matching the original <c>AggressiveBehavior</c>.
    /// </summary>
    public double MaxEngagementRange { get; set; } = 400_000;
}
```

### Step 2: Create FiringOutput

```csharp
using NpcMovementLib.Data;
using NpcMovementLib.Math;

namespace NpcWeaponLib.Data;

/// <summary>
/// Result of a single firing tick from <see cref="FiringSimulator.Tick"/>.
/// </summary>
public class FiringOutput
{
    /// <summary>Whether the NPC should fire this tick.</summary>
    public required bool ShouldFire { get; set; }

    /// <summary>
    /// The shot to dispatch if <see cref="ShouldFire"/> is true. Null otherwise.
    /// Contains all data needed to send the shot to the game server.
    /// </summary>
    public ShotData? Shot { get; set; }

    /// <summary>The weapon that was selected for this tick (even if not firing yet).</summary>
    public WeaponStats? SelectedWeapon { get; set; }

    /// <summary>Current fire interval in seconds (for diagnostics/UI).</summary>
    public double FireInterval { get; set; }

    /// <summary>Accumulated time since last shot in seconds (for diagnostics/UI).</summary>
    public double AccumulatedTime { get; set; }

    /// <summary>Ratio of functional weapons to total (0-1).</summary>
    public double FunctionalWeaponFactor { get; set; }

    /// <summary>Reason firing was suppressed, if <see cref="ShouldFire"/> is false.</summary>
    public FiringSuppressedReason? SuppressedReason { get; set; }
}

/// <summary>Why the NPC did not fire this tick.</summary>
public enum FiringSuppressedReason
{
    /// <summary>NPC is dead.</summary>
    NotAlive,
    /// <summary>No target assigned.</summary>
    NoTarget,
    /// <summary>No weapons on construct.</summary>
    NoWeapons,
    /// <summary>All weapons destroyed.</summary>
    AllWeaponsDestroyed,
    /// <summary>No compatible ammo found for configured tier/variant.</summary>
    NoCompatibleAmmo,
    /// <summary>Target beyond max engagement range.</summary>
    OutOfRange,
    /// <summary>Fire interval not yet reached — accumulating time.</summary>
    CooldownNotReached,
}
```

### Step 3: Create ShotData

All data needed to dispatch a shot to the game server. Replaces `ShootWeaponData` + `SentinelWeapon`.

```csharp
using NpcMovementLib.Data;
using NpcMovementLib.Math;

namespace NpcWeaponLib.Data;

/// <summary>
/// Complete shot data ready for dispatch to the game server.
/// Combines weapon properties (with modifiers applied) and positional context.
/// </summary>
/// <remarks>
/// This replaces both <c>ShootWeaponData</c> and the <c>SentinelWeapon</c> construction
/// from the original <c>AggressiveBehavior.ShootAndCycleAsync</c>.
/// The consumer's <see cref="Interfaces.IShotDispatchService"/> implementation
/// maps this to whatever the game server expects.
/// </remarks>
public class ShotData
{
    // --- Shooter Info ---
    public required string WeaponDisplayName { get; set; }
    public required ConstructId ShooterConstructId { get; set; }
    public required Vec3 ShooterPosition { get; set; }
    public required ulong ShooterConstructSize { get; set; }

    // --- Target Info ---
    public required ConstructId TargetConstructId { get; set; }
    public required Vec3 TargetPosition { get; set; }

    /// <summary>
    /// Local-space hit position on the target construct.
    /// Determined by <see cref="Interfaces.IHitPositionService"/> or random fallback.
    /// </summary>
    public required Vec3 HitPosition { get; set; }

    // --- Weapon Properties (modifiers already applied) ---
    public required double Damage { get; set; }
    public required double Range { get; set; }
    public required double BaseAccuracy { get; set; }
    public required double BaseOptimalDistance { get; set; }
    public required double BaseOptimalTracking { get; set; }
    public required double BaseOptimalAimingCone { get; set; }
    public required double FalloffDistance { get; set; }
    public required double FalloffTracking { get; set; }
    public required double FalloffAimingCone { get; set; }
    public required double OptimalCrossSectionDiameter { get; set; }
    public required double FireCooldown { get; set; }
    public required double CrossSection { get; set; }

    // --- Ammo ---
    public required string AmmoItemTypeName { get; set; }
    public required string WeaponItemTypeName { get; set; }

    /// <summary>Number of functional weapons firing simultaneously.</summary>
    public required int WeaponCount { get; set; }
}
```

### Step 4: Build and commit

Run: `dotnet build NpcWeaponLib/NpcWeaponLib.csproj`

```bash
git add NpcWeaponLib/Data/
git commit -m "feat: add firing input/output and shot data types"
```

---

## Task 4: Integration interfaces

**Files:**
- Create: `NpcWeaponLib/Interfaces/IWeaponHealthService.cs`
- Create: `NpcWeaponLib/Interfaces/IShotDispatchService.cs`
- Create: `NpcWeaponLib/Interfaces/IHitPositionService.cs`
- Create: `NpcWeaponLib/Interfaces/ISafeZoneService.cs`

### Step 1: Create all four interfaces

These define the boundary between the pure library and the game server, just like `NpcMovementLib.Interfaces`.

```csharp
// IWeaponHealthService.cs
using NpcMovementLib.Data;
using NpcWeaponLib.Data;

namespace NpcWeaponLib.Interfaces;

/// <summary>
/// Reads per-weapon health status from the game server.
/// </summary>
/// <remarks>
/// In the game backend, this queries construct elements via
/// <c>IConstructElementsService.GetDamagingWeaponsEffectiveness()</c>,
/// which reads hitpoint ratios from the Orleans element grains.
/// </remarks>
public interface IWeaponHealthService
{
    /// <summary>
    /// Returns weapon health data keyed by weapon ItemTypeName.
    /// Each value is a list of individual weapon elements of that type.
    /// </summary>
    Task<IDictionary<string, IList<WeaponEffectiveness>>> GetWeaponEffectiveness(ConstructId constructId);
}
```

```csharp
// IShotDispatchService.cs
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
```

```csharp
// IHitPositionService.cs
using NpcMovementLib.Data;
using NpcMovementLib.Math;

namespace NpcWeaponLib.Interfaces;

/// <summary>
/// Determines where on a target construct a shot will impact.
/// </summary>
/// <remarks>
/// In the game backend, this queries the voxel service (<c>IVoxelServiceClient.QueryRandomPoint</c>)
/// to find a valid surface point on the target. If the voxel service is unavailable,
/// a random direction scaled by construct size is used as fallback.
/// </remarks>
public interface IHitPositionService
{
    /// <summary>
    /// Returns a local-space hit position on the target construct.
    /// </summary>
    /// <param name="targetConstructId">Target to query.</param>
    /// <param name="shooterPosition">Shooter's world position (used to determine facing).</param>
    /// <returns>Hit position in target's local coordinate space.</returns>
    Task<Vec3> GetHitPositionAsync(ConstructId targetConstructId, Vec3 shooterPosition);
}
```

```csharp
// ISafeZoneService.cs
using NpcMovementLib.Data;

namespace NpcWeaponLib.Interfaces;

/// <summary>
/// Checks whether a construct is inside a safe zone (PvP-free area).
/// Firing is suppressed when either shooter or target is in a safe zone.
/// </summary>
public interface ISafeZoneService
{
    Task<bool> IsInSafeZoneAsync(ConstructId constructId);
}
```

### Step 2: Build and commit

Run: `dotnet build NpcWeaponLib/NpcWeaponLib.csproj`

```bash
git add NpcWeaponLib/Interfaces/
git commit -m "feat: add weapon integration interfaces"
```

---

## Task 5: FiringSimulator orchestrator

**Files:**
- Create: `NpcWeaponLib/FiringSimulator.cs`

### Step 1: Create FiringSimulator

This is the main entry point — analogous to `MovementSimulator.Tick()`. It owns the shot accumulator state and orchestrates weapon selection → ammo filtering → fire rate → shot data construction.

```csharp
namespace NpcWeaponLib;

using NpcWeaponLib.Data;

/// <summary>
/// Main entry point for NPC weapon firing simulation. Orchestrates weapon selection,
/// fire rate timing, and shot data construction into a single <see cref="Tick"/> call.
/// </summary>
/// <remarks>
/// <para>
/// Ported from <c>AggressiveBehavior.ShootAndCycleAsync()</c>. The original class
/// mixes DI, Orleans grains, voxel queries, and safe zone checks into a single method.
/// This version is a pure accumulator: it tracks elapsed time and returns whether
/// the NPC should fire, plus the complete <see cref="ShotData"/> if so.
/// </para>
/// <para>
/// Each call to <see cref="Tick"/> performs:
/// <list type="number">
///   <item>Validate inputs (alive, has target, has weapons).</item>
///   <item>Select best weapon for target distance via <see cref="WeaponSelector"/>.</item>
///   <item>Filter compatible ammo by tier and variant.</item>
///   <item>Compute fire interval via <see cref="WeaponFireRateCalculator"/>.</item>
///   <item>Accumulate delta time; if interval reached, produce <see cref="ShotData"/>.</item>
/// </list>
/// </para>
/// <para>
/// <b>Not handled by this class</b> (consumer responsibility):
/// <list type="bullet">
///   <item>Safe zone checks — call <see cref="Interfaces.ISafeZoneService"/> before/after Tick.</item>
///   <item>Hit position queries — call <see cref="Interfaces.IHitPositionService"/> and set on ShotData.</item>
///   <item>Shot dispatch — pass <see cref="ShotData"/> to <see cref="Interfaces.IShotDispatchService"/>.</item>
/// </list>
/// </para>
/// </remarks>
public class FiringSimulator
{
    private double _accumulatedTime;
    private readonly Random _random;

    public FiringSimulator(Random? random = null)
    {
        _random = random ?? new Random();
    }

    /// <summary>Current accumulated time since last shot, in seconds.</summary>
    public double AccumulatedTime => _accumulatedTime;

    /// <summary>
    /// Processes a single firing tick.
    /// </summary>
    public FiringOutput Tick(FiringInput input)
    {
        // --- Guard: not alive ---
        if (!input.IsAlive)
            return Suppressed(FiringSuppressedReason.NotAlive);

        // --- Guard: no target ---
        if (input.TargetConstructId is null || (ulong)input.TargetConstructId.Value == 0)
            return Suppressed(FiringSuppressedReason.NoTarget);

        // --- Guard: no weapons ---
        if (input.Weapons.Count == 0)
            return Suppressed(FiringSuppressedReason.NoWeapons);

        // --- Guard: all weapons destroyed ---
        if (!WeaponSelector.HasAnyFunctionalWeapons(input.WeaponEffectiveness))
            return Suppressed(FiringSuppressedReason.AllWeaponsDestroyed);

        // --- Range check ---
        var distance = input.Position.Dist(input.TargetPosition);
        if (distance > input.MaxEngagementRange)
            return Suppressed(FiringSuppressedReason.OutOfRange);

        // --- Select weapon ---
        var weapon = WeaponSelector.SelectBestWeapon(input.Weapons, input.WeaponEffectiveness, distance);
        if (weapon == null)
            return Suppressed(FiringSuppressedReason.AllWeaponsDestroyed);

        // --- Filter ammo ---
        var compatibleAmmo = weapon.AmmoItems
            .Where(a => a.Level == input.AmmoTier &&
                        a.ItemTypeName.Contains(input.AmmoVariant, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (compatibleAmmo.Count == 0)
            return Suppressed(FiringSuppressedReason.NoCompatibleAmmo, weapon);

        var ammo = compatibleAmmo[_random.Next(compatibleAmmo.Count)];

        // --- Effectiveness ---
        var (functionalCount, totalCount) = WeaponSelector.GetEffectivenessFactors(
            input.WeaponEffectiveness, weapon.ItemTypeName);
        var functionalFactor = Math.Clamp((double)functionalCount / totalCount, 0d, 1d);
        var clampedCount = Math.Clamp(functionalCount, 0, input.MaxWeaponCount);

        // --- Fire rate ---
        var fireInterval = WeaponFireRateCalculator.CalculateFireInterval(
            weapon, ammo, input.Modifiers, clampedCount, input.MaxWeaponCount);

        // --- Accumulate ---
        _accumulatedTime += input.DeltaTime;

        if (_accumulatedTime < fireInterval)
        {
            return new FiringOutput
            {
                ShouldFire = false,
                SelectedWeapon = weapon,
                FireInterval = fireInterval,
                AccumulatedTime = _accumulatedTime,
                FunctionalWeaponFactor = functionalFactor,
                SuppressedReason = FiringSuppressedReason.CooldownNotReached,
            };
        }

        // --- FIRE ---
        _accumulatedTime = 0;

        var mod = input.Modifiers;
        var shot = new ShotData
        {
            WeaponDisplayName = weapon.DisplayName,
            ShooterConstructId = input.ConstructId,
            ShooterPosition = input.Position,
            ShooterConstructSize = input.ConstructSize,
            TargetConstructId = input.TargetConstructId.Value,
            TargetPosition = input.TargetPosition,
            HitPosition = default, // Consumer sets via IHitPositionService

            Damage = weapon.BaseDamage * mod.Damage,
            Range = weapon.BaseOptimalDistance * mod.OptimalDistance + weapon.FalloffDistance * mod.FalloffDistance,
            BaseAccuracy = weapon.BaseAccuracy * mod.Accuracy,
            BaseOptimalDistance = weapon.BaseOptimalDistance * mod.OptimalDistance,
            BaseOptimalTracking = weapon.BaseOptimalTracking * mod.OptimalTracking,
            BaseOptimalAimingCone = weapon.BaseOptimalAimingCone * mod.OptimalAimingCone,
            FalloffDistance = weapon.FalloffDistance * mod.FalloffDistance,
            FalloffTracking = weapon.FalloffTracking * mod.FalloffTracking,
            FalloffAimingCone = weapon.FalloffAimingCone * mod.FalloffAimingCone,
            OptimalCrossSectionDiameter = weapon.OptimalCrossSectionDiameter,
            FireCooldown = fireInterval,
            CrossSection = 5,

            AmmoItemTypeName = ammo.ItemTypeName,
            WeaponItemTypeName = weapon.ItemTypeName,
            WeaponCount = clampedCount,
        };

        return new FiringOutput
        {
            ShouldFire = true,
            Shot = shot,
            SelectedWeapon = weapon,
            FireInterval = fireInterval,
            AccumulatedTime = 0,
            FunctionalWeaponFactor = functionalFactor,
        };
    }

    /// <summary>Resets the shot accumulator (e.g., when target changes or safe zone entered).</summary>
    public void ResetAccumulator() => _accumulatedTime = 0;

    private static FiringOutput Suppressed(FiringSuppressedReason reason, WeaponStats? weapon = null)
    {
        return new FiringOutput
        {
            ShouldFire = false,
            SuppressedReason = reason,
            SelectedWeapon = weapon,
        };
    }
}
```

### Step 2: Build and commit

Run: `dotnet build NpcWeaponLib/NpcWeaponLib.csproj`

```bash
git add NpcWeaponLib/FiringSimulator.cs
git commit -m "feat: add FiringSimulator orchestrator"
```

---

## Task 6: XML documentation pass

**Files:**
- Modify: All files in `NpcWeaponLib/`

### Step 1: Add comprehensive XML docs

Same standard as NpcMovementLib — every public class, interface, property, method, and enum value gets `/// <summary>`, `/// <remarks>` where appropriate, `/// <param>`, `/// <returns>`. Cross-reference original backend source files. Include units (metres, seconds, etc.).

Most docs are already inline in the code above, but do a completeness pass to ensure nothing was missed.

### Step 2: Build and commit

Run: `dotnet build NpcWeaponLib/NpcWeaponLib.csproj`

```bash
git add NpcWeaponLib/
git commit -m "docs: add comprehensive XML documentation to NpcWeaponLib"
```

---

## Task 7: README

**Files:**
- Create: `NpcWeaponLib/README.md`

### Step 1: Write README

Follow the same structure as `NpcMovementLib/README.md`:
1. Title + overview
2. Features bullet list
3. Getting Started (project ref + basic usage example)
4. Component Design — Mermaid flowchart showing: FiringInput → FiringSimulator → WeaponSelector → ammo filter → fire rate calc → accumulator → FiringOutput/ShotData
5. Architecture — Mermaid class diagram
6. Integration Interfaces section
7. Fire Rate Pipeline section with the formula chain

### Step 2: Commit

```bash
git add NpcWeaponLib/README.md
git commit -m "docs: add NpcWeaponLib README with Mermaid diagrams"
```

---

## Task 8: Code review

### Step 1: Cross-reference review

Use the code-reviewer agent to verify:
- All fire rate formulas match `WeaponItem.cs` exactly
- Weapon selection logic matches `BehaviorContext.GetBestFunctionalWeaponByTargetDistance()` exactly
- Modifier application matches `AggressiveBehavior.ShootAndCycleAsync()` exactly
- No game-server dependencies leaked in
- No PrescriberPoint / healthcare references
- Build passes with 0 warnings

### Step 2: Fix any issues and commit

```bash
git add NpcWeaponLib/
git commit -m "fix: address code review findings"
```
