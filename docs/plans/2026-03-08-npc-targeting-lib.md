# NPC Targeting & Common Library Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Extract NPC target selection, threat assessment, and lead prediction into an isolated `NpcTargetingLib`, and refactor shared types (`Vec3`, `ConstructId`, `ScanContact`) into a new `NpcCommonLib` that all three NPC libraries depend on.

**Architecture:** Three-phase approach: (1) Create `NpcCommonLib` with shared math and data types, (2) Migrate `NpcMovementLib` and `NpcWeaponLib` to reference it instead of duplicating/cross-referencing, (3) Build `NpcTargetingLib` with pure targeting strategies, damage tracking, threat calculation, and lead prediction. All libraries remain zero-dependency beyond BCL and each other. The dependency graph becomes:

```
NpcCommonLib  (Vec3, ConstructId, ScanContact, IRadarService)
    ↑              ↑                ↑
NpcMovementLib  NpcWeaponLib   NpcTargetingLib
```

**Tech Stack:** C# / .NET 8.0, no external NuGet dependencies (BCL only).

---

## Why NpcCommonLib?

Currently `NpcWeaponLib` references `NpcMovementLib` solely for `Vec3`, `ConstructId`, and `ScanContact`. The new `NpcTargetingLib` would also need these same types plus `IRadarService`. These aren't movement-specific — they're shared NPC domain types. Without a common lib, we'd either:
- Create a circular dependency (targeting needs movement types, movement shouldn't depend on targeting)
- Duplicate types across libraries

The refactor is mechanical: move files, update namespaces, update project references.

---

## Task 1: Create NpcCommonLib and move shared types

**Files:**
- Create: `NpcCommonLib/NpcCommonLib.csproj`
- Move: `NpcMovementLib/Math/Vec3.cs` → `NpcCommonLib/Math/Vec3.cs`
- Move: `NpcMovementLib/Data/ConstructId.cs` → `NpcCommonLib/Data/ConstructId.cs`
- Move: `NpcMovementLib/Data/ScanContact.cs` → `NpcCommonLib/Data/ScanContact.cs`
- Move: `NpcMovementLib/Data/ConstructTransformResult.cs` → `NpcCommonLib/Data/ConstructTransformResult.cs`
- Move: `NpcMovementLib/Data/ConstructVelocityResult.cs` → `NpcCommonLib/Data/ConstructVelocityResult.cs`
- Move: `NpcMovementLib/Interfaces/IRadarService.cs` → `NpcCommonLib/Interfaces/IRadarService.cs`
- Move: `NpcMovementLib/Interfaces/IConstructService.cs` → `NpcCommonLib/Interfaces/IConstructService.cs`
- Move: `NpcMovementLib/Interfaces/IConstructUpdateService.cs` → `NpcCommonLib/Interfaces/IConstructUpdateService.cs`
- Modify: `NpcMovementLib/NpcMovementLib.csproj` — add reference to NpcCommonLib
- Modify: `NpcWeaponLib/NpcWeaponLib.csproj` — change reference from NpcMovementLib to NpcCommonLib
- Modify: All `using` statements across both libraries

### Step 1: Create NpcCommonLib.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>NpcCommonLib</RootNamespace>
  </PropertyGroup>
</Project>
```

### Step 2: Move files and update namespaces

Move the following files, changing their namespace from `NpcMovementLib.*` to `NpcCommonLib.*`:

| Source | Destination | Old Namespace | New Namespace |
|--------|-------------|---------------|---------------|
| `NpcMovementLib/Math/Vec3.cs` | `NpcCommonLib/Math/Vec3.cs` | `NpcMovementLib.Math` | `NpcCommonLib.Math` |
| `NpcMovementLib/Data/ConstructId.cs` | `NpcCommonLib/Data/ConstructId.cs` | `NpcMovementLib.Data` | `NpcCommonLib.Data` |
| `NpcMovementLib/Data/ScanContact.cs` | `NpcCommonLib/Data/ScanContact.cs` | `NpcMovementLib.Data` | `NpcCommonLib.Data` |
| `NpcMovementLib/Data/ConstructTransformResult.cs` | `NpcCommonLib/Data/ConstructTransformResult.cs` | `NpcMovementLib.Data` | `NpcCommonLib.Data` |
| `NpcMovementLib/Data/ConstructVelocityResult.cs` | `NpcCommonLib/Data/ConstructVelocityResult.cs` | `NpcMovementLib.Data` | `NpcCommonLib.Data` |
| `NpcMovementLib/Interfaces/IRadarService.cs` | `NpcCommonLib/Interfaces/IRadarService.cs` | `NpcMovementLib.Interfaces` | `NpcCommonLib.Interfaces` |
| `NpcMovementLib/Interfaces/IConstructService.cs` | `NpcCommonLib/Interfaces/IConstructService.cs` | `NpcMovementLib.Interfaces` | `NpcCommonLib.Interfaces` |
| `NpcMovementLib/Interfaces/IConstructUpdateService.cs` | `NpcCommonLib/Interfaces/IConstructUpdateService.cs` | `NpcMovementLib.Interfaces` | `NpcCommonLib.Interfaces` |

### Step 3: Update NpcMovementLib.csproj to reference NpcCommonLib

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>NpcMovementLib</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\NpcCommonLib\NpcCommonLib.csproj" />
  </ItemGroup>
</Project>
```

### Step 4: Update all `using` statements in NpcMovementLib

Replace across all remaining NpcMovementLib files:
- `using NpcMovementLib.Math;` → `using NpcCommonLib.Math;`
- `using NpcMovementLib.Data;` → `using NpcCommonLib.Data;` (where it references moved types)
- `using NpcMovementLib.Interfaces;` → `using NpcCommonLib.Interfaces;` (where it references moved interfaces)

**Note:** NpcMovementLib's own types (MovementInput, MovementOutput, MovementConfig, VelocityModifiers) stay in `NpcMovementLib.Data`. Only types that are shared across libraries move.

### Step 5: Update NpcWeaponLib.csproj

Change the project reference from NpcMovementLib to NpcCommonLib (NpcWeaponLib only uses Vec3 and ConstructId from movement lib — it doesn't need movement strategies):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\NpcCommonLib\NpcCommonLib.csproj" />
  </ItemGroup>
</Project>
```

### Step 6: Update all `using` statements in NpcWeaponLib

Replace across all NpcWeaponLib files:
- `using NpcMovementLib.Math;` → `using NpcCommonLib.Math;`
- `using NpcMovementLib.Data;` → `using NpcCommonLib.Data;`

### Step 7: Build all three projects

```bash
dotnet build NpcCommonLib/NpcCommonLib.csproj
dotnet build NpcMovementLib/NpcMovementLib.csproj
dotnet build NpcWeaponLib/NpcWeaponLib.csproj
```

All must pass with 0 errors, 0 warnings.

### Step 8: Commit

```bash
git add NpcCommonLib/ NpcMovementLib/ NpcWeaponLib/
git commit -m "refactor: extract NpcCommonLib with shared types (Vec3, ConstructId, ScanContact, interfaces)

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

---

## Task 2: Create NpcTargetingLib project and data types

**Files:**
- Create: `NpcTargetingLib/NpcTargetingLib.csproj`
- Create: `NpcTargetingLib/Data/DamageEvent.cs`
- Create: `NpcTargetingLib/Data/TargetingInput.cs`
- Create: `NpcTargetingLib/Data/TargetingOutput.cs`

### Step 1: Create NpcTargetingLib.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\NpcCommonLib\NpcCommonLib.csproj" />
  </ItemGroup>
</Project>
```

### Step 2: Create DamageEvent

Port from `Backend/Features/Spawner/Data/DamageDealtData.cs`. Drop the game-specific constructor.

```csharp
using NpcCommonLib.Data;

namespace NpcTargetingLib.Data;

/// <summary>
/// Records a single damage event dealt to the NPC by an attacker.
/// </summary>
/// <remarks>
/// Ported from <c>Mod.DynamicEncounters.Features.Spawner.Data.DamageDealtData</c>.
/// Used by <see cref="DamageTracker"/> to maintain a rolling damage history
/// for threat assessment. The original is registered via an HTTP endpoint
/// (<c>BehaviorContextController.RegisterDamage</c>) when the game detects
/// a shot impact on this NPC.
/// </remarks>
public class DamageEvent
{
    /// <summary>Construct ID of the attacker that dealt this damage.</summary>
    public required ConstructId AttackerConstructId { get; set; }

    /// <summary>Player ID of the attacker (0 if NPC-on-NPC damage).</summary>
    public required ulong AttackerPlayerId { get; set; }

    /// <summary>Damage amount dealt in this event.</summary>
    public required double Damage { get; set; }

    /// <summary>
    /// Damage type descriptor (e.g., "shield-hit", "kinetic", "thermic").
    /// Used for logging and analytics, not for targeting decisions.
    /// </summary>
    public required string Type { get; set; }

    /// <summary>UTC timestamp when this damage was dealt.</summary>
    public required DateTime Timestamp { get; set; }
}
```

### Step 3: Create TargetingInput

```csharp
using NpcCommonLib.Data;
using NpcCommonLib.Math;

namespace NpcTargetingLib.Data;

/// <summary>
/// All inputs needed for a single targeting tick.
/// </summary>
public class TargetingInput
{
    /// <summary>NPC's own construct identifier (excluded from radar results).</summary>
    public required ConstructId ConstructId { get; set; }

    /// <summary>NPC's current world-space position in metres.</summary>
    public required Vec3 Position { get; set; }

    /// <summary>NPC's home/spawn position — used as fallback move target when no contacts.</summary>
    public required Vec3 StartPosition { get; set; }

    /// <summary>Radar contacts from the most recent scan.</summary>
    public required IReadOnlyList<ScanContact> Contacts { get; set; }

    /// <summary>Seconds since last tick.</summary>
    public required double DeltaTime { get; set; }

    /// <summary>
    /// Target's linear velocity in m/s (for lead prediction).
    /// Zero vector if no target or velocity unknown.
    /// </summary>
    public Vec3 TargetLinearVelocity { get; set; }

    /// <summary>
    /// Target's acceleration in m/s² (for lead prediction).
    /// Zero vector if unknown.
    /// </summary>
    public Vec3 TargetAcceleration { get; set; }

    /// <summary>
    /// Weapon optimal range in metres. Used to compute prediction seconds
    /// and to determine whether target is inside/outside optimal range.
    /// </summary>
    public double WeaponOptimalRange { get; set; }

    /// <summary>
    /// Maximum visibility distance in metres. Targets beyond this are ignored.
    /// Default: 10 SU (2,000,000 m).
    /// </summary>
    public double MaxVisibilityDistance { get; set; } = 2_000_000;

    /// <summary>
    /// How long a random target selection is held before re-rolling, in seconds.
    /// Only used by <see cref="Strategies.RandomTargetStrategy"/>. Default: 30s.
    /// </summary>
    public double DecisionHoldSeconds { get; set; } = 30;
}
```

### Step 4: Create TargetingOutput

```csharp
using NpcCommonLib.Data;
using NpcCommonLib.Math;

namespace NpcTargetingLib.Data;

/// <summary>
/// Result of a single targeting tick from <see cref="TargetingSimulator.Tick"/>.
/// </summary>
public class TargetingOutput
{
    /// <summary>Whether a valid target was selected.</summary>
    public required bool HasTarget { get; set; }

    /// <summary>The selected target's construct ID. Null if no target.</summary>
    public ConstructId? TargetConstructId { get; set; }

    /// <summary>
    /// The position the NPC should move toward. This may include lead prediction
    /// offset and random jitter — it is NOT simply the target's raw position.
    /// </summary>
    public Vec3 MoveToPosition { get; set; }

    /// <summary>
    /// The target's raw position (without prediction/offset).
    /// Used for distance calculations and weapon ranging.
    /// </summary>
    public Vec3 TargetPosition { get; set; }

    /// <summary>Distance from NPC to target in metres.</summary>
    public double TargetDistance { get; set; }

    /// <summary>
    /// Prediction seconds used for lead calculation this tick.
    /// 10s (far), 30s (medium), 60s (close to optimal range).
    /// </summary>
    public double PredictionSeconds { get; set; }

    /// <summary>Reason no target was selected, if <see cref="HasTarget"/> is false.</summary>
    public NoTargetReason? Reason { get; set; }
}

/// <summary>Why no target was selected this tick.</summary>
public enum NoTargetReason
{
    /// <summary>No radar contacts within scan range.</summary>
    NoContacts,
    /// <summary>All contacts are beyond max visibility distance.</summary>
    AllOutOfRange,
    /// <summary>Target construct no longer exists in contacts list.</summary>
    TargetLost,
}
```

### Step 5: Build and commit

```bash
dotnet build NpcTargetingLib/NpcTargetingLib.csproj
```

```bash
git add NpcTargetingLib/
git commit -m "feat: add NpcTargetingLib project with data types

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

---

## Task 3: DamageTracker and ThreatCalculator

**Files:**
- Create: `NpcTargetingLib/DamageTracker.cs`
- Create: `NpcTargetingLib/ThreatCalculator.cs`

### Step 1: Create DamageTracker

Port the rolling damage history from `BehaviorContext.DamageHistory` + `RegisterDamage` + `GetRecentDamageHistory`.

```csharp
using NpcCommonLib.Data;
using NpcTargetingLib.Data;

namespace NpcTargetingLib;

/// <summary>
/// Thread-safe rolling damage history for an NPC construct.
/// Tracks who dealt damage and when, for threat assessment.
/// </summary>
/// <remarks>
/// Ported from <c>BehaviorContext.DamageHistory</c> (a <c>ConcurrentBag</c>)
/// and <c>RegisterDamage()</c> / <c>GetRecentDamageHistory()</c>.
/// The original prunes to a 10-minute window on each registration;
/// this version does the same.
/// </remarks>
public class DamageTracker
{
    private readonly object _lock = new();
    private List<DamageEvent> _events = [];

    /// <summary>
    /// How far back to retain damage events. Default: 10 minutes
    /// (matching the original <c>BehaviorContext</c>).
    /// </summary>
    public TimeSpan RetentionWindow { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Registers a damage event and prunes expired entries.
    /// Thread-safe.
    /// </summary>
    public void RegisterDamage(DamageEvent damage)
    {
        lock (_lock)
        {
            var cutoff = DateTime.UtcNow - RetentionWindow;
            _events = _events.Where(e => e.Timestamp > cutoff).ToList();
            _events.Add(damage);
        }
    }

    /// <summary>
    /// Returns all damage events within the retention window.
    /// </summary>
    public IReadOnlyList<DamageEvent> GetRecentHistory()
    {
        lock (_lock)
        {
            var cutoff = DateTime.UtcNow - RetentionWindow;
            return _events.Where(e => e.Timestamp > cutoff).ToList();
        }
    }

    /// <summary>
    /// Returns all damage events within a custom time window.
    /// </summary>
    /// <param name="window">How far back to look.</param>
    public IReadOnlyList<DamageEvent> GetHistory(TimeSpan window)
    {
        lock (_lock)
        {
            var cutoff = DateTime.UtcNow - window;
            return _events.Where(e => e.Timestamp > cutoff).ToList();
        }
    }

    /// <summary>Clears all damage history.</summary>
    public void Clear()
    {
        lock (_lock) { _events.Clear(); }
    }
}
```

### Step 2: Create ThreatCalculator

Port from `BehaviorContext.GetHighestThreatConstruct()`. Pure static function.

```csharp
using NpcCommonLib.Data;
using NpcTargetingLib.Data;

namespace NpcTargetingLib;

/// <summary>
/// Calculates which attacker poses the highest threat based on recent damage history.
/// Pure static functions with no side effects.
/// </summary>
/// <remarks>
/// Ported from <c>BehaviorContext.GetHighestThreatConstruct()</c>.
/// Uses a 1-minute window (not the 10-minute retention window) for threat ranking,
/// falling back to the closest radar contact if no recent damage exists.
/// </remarks>
public static class ThreatCalculator
{
    /// <summary>
    /// Threat assessment window. The original uses 1 minute for threat ranking
    /// (shorter than the 10-minute damage retention window).
    /// </summary>
    public static readonly TimeSpan DefaultThreatWindow = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Returns the construct ID that dealt the most total damage within the threat window.
    /// Falls back to the closest contact if no damage was received recently.
    /// Returns null if no contacts and no damage history.
    /// </summary>
    /// <param name="damageHistory">Recent damage events (from <see cref="DamageTracker"/>).</param>
    /// <param name="contacts">Current radar contacts.</param>
    /// <param name="threatWindow">How far back to consider damage. Default: 1 minute.</param>
    public static ConstructId? GetHighestThreat(
        IReadOnlyList<DamageEvent> damageHistory,
        IReadOnlyList<ScanContact> contacts,
        TimeSpan? threatWindow = null)
    {
        var window = threatWindow ?? DefaultThreatWindow;
        var cutoff = DateTime.UtcNow - window;

        var highestThreat = damageHistory
            .Where(e => e.Timestamp > cutoff)
            .GroupBy(e => e.AttackerConstructId)
            .Select(g => new { ConstructId = g.Key, TotalDamage = g.Sum(e => e.Damage) })
            .OrderByDescending(x => x.TotalDamage)
            .FirstOrDefault();

        if (highestThreat != null)
            return highestThreat.ConstructId;

        // Fallback: closest contact
        return GetClosestContact(contacts);
    }

    /// <summary>
    /// Returns the construct ID of the nearest radar contact, or null if none.
    /// </summary>
    public static ConstructId? GetClosestContact(IReadOnlyList<ScanContact> contacts)
    {
        if (contacts.Count == 0) return null;
        return contacts.MinBy(c => c.Distance)?.ConstructId;
    }
}
```

### Step 3: Build and commit

```bash
dotnet build NpcTargetingLib/NpcTargetingLib.csproj
```

```bash
git add NpcTargetingLib/DamageTracker.cs NpcTargetingLib/ThreatCalculator.cs
git commit -m "feat: add DamageTracker and ThreatCalculator

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

---

## Task 4: Target selection strategies

**Files:**
- Create: `NpcTargetingLib/Strategies/ITargetSelectionStrategy.cs`
- Create: `NpcTargetingLib/Strategies/ClosestTargetStrategy.cs`
- Create: `NpcTargetingLib/Strategies/HighestThreatTargetStrategy.cs`
- Create: `NpcTargetingLib/Strategies/RandomTargetStrategy.cs`

### Step 1: Create ITargetSelectionStrategy

```csharp
using NpcCommonLib.Data;

namespace NpcTargetingLib.Strategies;

/// <summary>
/// Strategy interface for selecting a target from radar contacts.
/// </summary>
/// <remarks>
/// Ported from <c>ISelectRadarTargetEffect</c>. The original uses the "effect" system
/// with a <c>BehaviorContext</c> parameter; this version uses pure inputs.
/// </remarks>
public interface ITargetSelectionStrategy
{
    /// <summary>
    /// Selects a target from the available contacts.
    /// Returns null if no suitable target found.
    /// </summary>
    /// <param name="params">Selection parameters including contacts and damage history.</param>
    ScanContact? SelectTarget(TargetSelectionParams @params);
}

/// <summary>
/// Input parameters for target selection strategies.
/// </summary>
public class TargetSelectionParams
{
    /// <summary>Current radar contacts, sorted by ascending distance.</summary>
    public required IReadOnlyList<ScanContact> Contacts { get; set; }

    /// <summary>Recent damage history for threat-based selection.</summary>
    public required IReadOnlyList<DamageEvent> DamageHistory { get; set; }

    /// <summary>Seconds since last tick (for time-based strategies like Random hold).</summary>
    public required double DeltaTime { get; set; }

    /// <summary>How long to hold a random selection before re-rolling. Default: 30s.</summary>
    public double DecisionHoldSeconds { get; set; } = 30;
}
```

### Step 2: Create ClosestTargetStrategy

```csharp
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
        return @params.Contacts.MinBy(c => c.Distance);
    }
}
```

### Step 3: Create HighestThreatTargetStrategy

```csharp
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
    public ScanContact? SelectTarget(TargetSelectionParams @params)
    {
        var threatId = ThreatCalculator.GetHighestThreat(
            @params.DamageHistory, @params.Contacts);

        if (threatId == null) return null;

        // Return the contact matching the threat — it must still be on radar
        return @params.Contacts.FirstOrDefault(c => c.ConstructId == threatId.Value);
    }
}
```

### Step 4: Create RandomTargetStrategy

```csharp
using NpcCommonLib.Data;

namespace NpcTargetingLib.Strategies;

/// <summary>
/// Selects a random contact and holds that selection for a configurable duration.
/// </summary>
/// <remarks>
/// Ported from <c>RandomSelectRadarTargetEffect</c>. Stateful — tracks
/// accumulated time and last selection. Re-rolls after <c>DecisionHoldSeconds</c>.
/// </remarks>
public class RandomTargetStrategy : ITargetSelectionStrategy
{
    private readonly Random _random;
    private double _accumulatedTime;
    private ConstructId? _lastSelectedId;

    public RandomTargetStrategy(Random? random = null)
    {
        _random = random ?? new Random();
    }

    public ScanContact? SelectTarget(TargetSelectionParams @params)
    {
        _accumulatedTime += @params.DeltaTime;

        // Check if current selection is still valid (on radar)
        ScanContact? current = null;
        if (_lastSelectedId != null)
            current = @params.Contacts.FirstOrDefault(c => c.ConstructId == _lastSelectedId.Value);

        // Re-roll if: no selection, selection lost, or hold time expired
        if (current == null || _accumulatedTime > @params.DecisionHoldSeconds)
        {
            if (@params.Contacts.Count == 0)
            {
                _lastSelectedId = null;
                _accumulatedTime = 0;
                return null;
            }

            var index = _random.Next(@params.Contacts.Count);
            current = @params.Contacts[index];
            _lastSelectedId = current.ConstructId;
            _accumulatedTime = 0;
        }

        return current;
    }
}
```

### Step 5: Build and commit

```bash
dotnet build NpcTargetingLib/NpcTargetingLib.csproj
```

```bash
git add NpcTargetingLib/Strategies/
git commit -m "feat: add target selection strategies (Closest, HighestThreat, Random)

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

---

## Task 5: Lead prediction and move position calculator

**Files:**
- Create: `NpcTargetingLib/LeadPredictor.cs`
- Create: `NpcTargetingLib/MovePositionCalculator.cs`

### Step 1: Create LeadPredictor

Wraps the kinematic future position calculation with range-tiered prediction seconds.

```csharp
using NpcCommonLib.Math;

namespace NpcTargetingLib;

/// <summary>
/// Predicts a target's future position using kinematic equations.
/// </summary>
/// <remarks>
/// <para>
/// Uses the standard kinematic equation: <c>p = p0 + v*t + 0.5*a*t^2</c>
/// where t is the prediction time in seconds.
/// </para>
/// <para>
/// Prediction time varies by range tier (matching <c>BehaviorContext.CalculateMovementPredictionSeconds()</c>):
/// <list type="bullet">
///   <item>Outside 2x optimal range: 10 seconds (fast-closing, less prediction needed)</item>
///   <item>Outside 1x optimal range: 30 seconds (moderate prediction)</item>
///   <item>Inside optimal range: 60 seconds (tight engagement, max prediction)</item>
/// </list>
/// </para>
/// </remarks>
public static class LeadPredictor
{
    /// <summary>
    /// Predicts target position at a future time using kinematic equation.
    /// </summary>
    /// <param name="currentPosition">Target's current position in metres.</param>
    /// <param name="velocity">Target's velocity in m/s.</param>
    /// <param name="acceleration">Target's acceleration in m/s².</param>
    /// <param name="predictionSeconds">How far ahead to predict, in seconds.</param>
    /// <returns>Predicted future position in metres.</returns>
    public static Vec3 PredictFuturePosition(
        Vec3 currentPosition, Vec3 velocity, Vec3 acceleration, double predictionSeconds)
    {
        var t = predictionSeconds;
        return new Vec3(
            currentPosition.X + velocity.X * t + 0.5 * acceleration.X * t * t,
            currentPosition.Y + velocity.Y * t + 0.5 * acceleration.Y * t * t,
            currentPosition.Z + velocity.Z * t + 0.5 * acceleration.Z * t * t
        );
    }

    /// <summary>
    /// Calculates prediction seconds based on distance to target relative to weapon optimal range.
    /// </summary>
    /// <param name="distanceToTarget">Distance to target in metres.</param>
    /// <param name="weaponOptimalRange">Weapon's optimal engagement range in metres.</param>
    /// <returns>10, 30, or 60 seconds.</returns>
    public static double CalculatePredictionSeconds(double distanceToTarget, double weaponOptimalRange)
    {
        if (weaponOptimalRange <= 0) return 10;

        if (distanceToTarget > 2 * weaponOptimalRange) return 10;
        if (distanceToTarget > weaponOptimalRange) return 30;
        return 60;
    }
}
```

### Step 2: Create MovePositionCalculator

Port from `CalculateTargetMovePositionWithOffsetEffect`. The pure part: random offset + optional lead prediction.

```csharp
using NpcCommonLib.Math;

namespace NpcTargetingLib;

/// <summary>
/// Calculates the move-to position for the NPC, combining target position
/// with random offset and optional lead prediction.
/// </summary>
/// <remarks>
/// Ported from <c>CalculateTargetMovePositionWithOffsetEffect</c>.
/// The original queries the game server for target position; this version
/// takes the target position as input. The random offset is regenerated
/// every 30 seconds to prevent the NPC from flying in a straight line.
/// </remarks>
public class MovePositionCalculator
{
    private readonly Random _random;
    private Vec3 _offset;
    private DateTime? _lastOffsetUpdate;

    /// <summary>How often to regenerate the random offset. Default: 30 seconds.</summary>
    public TimeSpan OffsetRefreshInterval { get; set; } = TimeSpan.FromSeconds(30);

    public MovePositionCalculator(Random? random = null)
    {
        _random = random ?? new Random();
    }

    /// <summary>
    /// Calculates the move-to position given the target's position and movement data.
    /// </summary>
    /// <param name="targetPosition">Target's current position in metres.</param>
    /// <param name="targetVelocity">Target's velocity in m/s.</param>
    /// <param name="targetAcceleration">Target's acceleration in m/s².</param>
    /// <param name="predictionSeconds">Lead prediction lookahead time.</param>
    /// <param name="approachDistance">Desired engagement distance in metres (offset magnitude).</param>
    /// <param name="usePrediction">Whether to apply lead prediction. Default: false (matching current backend which has it commented out).</param>
    /// <returns>The position the NPC should navigate toward.</returns>
    public Vec3 Calculate(
        Vec3 targetPosition,
        Vec3 targetVelocity,
        Vec3 targetAcceleration,
        double predictionSeconds,
        double approachDistance,
        bool usePrediction = false)
    {
        // Refresh random offset periodically
        var now = DateTime.UtcNow;
        if (_lastOffsetUpdate == null || (now - _lastOffsetUpdate.Value) > OffsetRefreshInterval)
        {
            _offset = RandomDirectionVec3() * (approachDistance / 2);
            _lastOffsetUpdate = now;
        }

        var basePosition = targetPosition;

        if (usePrediction)
        {
            basePosition = LeadPredictor.PredictFuturePosition(
                targetPosition, targetVelocity, targetAcceleration, predictionSeconds);
        }

        return basePosition + _offset;
    }

    /// <summary>Resets the offset timer, forcing a new offset on next call.</summary>
    public void ResetOffset() => _lastOffsetUpdate = null;

    private Vec3 RandomDirectionVec3()
    {
        // Generate a random unit vector (uniform on sphere)
        var theta = _random.NextDouble() * 2 * Math.PI;
        var phi = Math.Acos(2 * _random.NextDouble() - 1);
        return new Vec3(
            Math.Sin(phi) * Math.Cos(theta),
            Math.Sin(phi) * Math.Sin(theta),
            Math.Cos(phi)
        );
    }
}
```

### Step 3: Build and commit

```bash
dotnet build NpcTargetingLib/NpcTargetingLib.csproj
```

```bash
git add NpcTargetingLib/LeadPredictor.cs NpcTargetingLib/MovePositionCalculator.cs
git commit -m "feat: add lead prediction and move position calculator

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

---

## Task 6: TargetingSimulator orchestrator

**Files:**
- Create: `NpcTargetingLib/TargetingSimulator.cs`

### Step 1: Create TargetingSimulator

Main entry point that ties everything together: strategy selection → target pick → move position calculation → output.

```csharp
using NpcCommonLib.Data;
using NpcTargetingLib.Data;
using NpcTargetingLib.Strategies;

namespace NpcTargetingLib;

/// <summary>
/// Main entry point for NPC targeting simulation. Orchestrates target selection,
/// threat assessment, and move position calculation into a single <see cref="Tick"/> call.
/// </summary>
/// <remarks>
/// <para>
/// Ported from <c>SelectTargetBehavior.TickAsync()</c>. The original mixes DI,
/// radar scanning, safe zone filtering, and game-server calls. This version
/// is pure: it takes pre-scanned contacts and damage history as input.
/// </para>
/// <para>
/// Each call to <see cref="Tick"/> performs:
/// <list type="number">
///   <item>Run the <see cref="ITargetSelectionStrategy"/> to pick a target from contacts.</item>
///   <item>Calculate prediction seconds based on distance vs weapon optimal range.</item>
///   <item>Compute move-to position via <see cref="MovePositionCalculator"/>.</item>
///   <item>Return <see cref="TargetingOutput"/> with selected target, move position, and diagnostics.</item>
/// </list>
/// </para>
/// </remarks>
public class TargetingSimulator
{
    private readonly ITargetSelectionStrategy _strategy;
    private readonly MovePositionCalculator _moveCalculator;
    private readonly DamageTracker _damageTracker;

    /// <summary>
    /// Creates a targeting simulator with the specified strategy.
    /// Defaults to <see cref="HighestThreatTargetStrategy"/> (matching the game's default).
    /// </summary>
    public TargetingSimulator(
        ITargetSelectionStrategy? strategy = null,
        DamageTracker? damageTracker = null,
        MovePositionCalculator? moveCalculator = null)
    {
        _strategy = strategy ?? new HighestThreatTargetStrategy();
        _damageTracker = damageTracker ?? new DamageTracker();
        _moveCalculator = moveCalculator ?? new MovePositionCalculator();
    }

    /// <summary>The damage tracker used for threat assessment. Expose so consumers can register damage.</summary>
    public DamageTracker DamageTracker => _damageTracker;

    /// <summary>
    /// Processes a single targeting tick.
    /// </summary>
    public TargetingOutput Tick(TargetingInput input)
    {
        // --- No contacts ---
        if (input.Contacts.Count == 0)
        {
            return new TargetingOutput
            {
                HasTarget = false,
                Reason = NoTargetReason.NoContacts,
                MoveToPosition = input.StartPosition,
            };
        }

        // --- Run strategy ---
        var selectionParams = new TargetSelectionParams
        {
            Contacts = input.Contacts,
            DamageHistory = _damageTracker.GetRecentHistory(),
            DeltaTime = input.DeltaTime,
            DecisionHoldSeconds = input.DecisionHoldSeconds,
        };

        var selected = _strategy.SelectTarget(selectionParams);

        if (selected == null)
        {
            return new TargetingOutput
            {
                HasTarget = false,
                Reason = NoTargetReason.NoContacts,
                MoveToPosition = input.StartPosition,
            };
        }

        // --- Visibility check ---
        if (selected.Distance > input.MaxVisibilityDistance)
        {
            return new TargetingOutput
            {
                HasTarget = false,
                Reason = NoTargetReason.AllOutOfRange,
                MoveToPosition = input.StartPosition,
            };
        }

        // --- Prediction ---
        var predictionSeconds = LeadPredictor.CalculatePredictionSeconds(
            selected.Distance, input.WeaponOptimalRange);

        var moveToPosition = _moveCalculator.Calculate(
            targetPosition: selected.Position,
            targetVelocity: input.TargetLinearVelocity,
            targetAcceleration: input.TargetAcceleration,
            predictionSeconds: predictionSeconds,
            approachDistance: input.WeaponOptimalRange
        );

        return new TargetingOutput
        {
            HasTarget = true,
            TargetConstructId = selected.ConstructId,
            TargetPosition = selected.Position,
            MoveToPosition = moveToPosition,
            TargetDistance = selected.Distance,
            PredictionSeconds = predictionSeconds,
        };
    }
}
```

### Step 2: Build and commit

```bash
dotnet build NpcTargetingLib/NpcTargetingLib.csproj
```

```bash
git add NpcTargetingLib/TargetingSimulator.cs
git commit -m "feat: add TargetingSimulator orchestrator

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

---

## Task 7: XML documentation pass

**Files:**
- Modify: All files in `NpcTargetingLib/`
- Modify: All files in `NpcCommonLib/` (ensure moved files have updated cross-references)

### Step 1: Completeness pass

Read every file in both libraries. Ensure every public member has XML docs. The plan code above already has most docs inline — verify nothing was missed. Update any `<see cref="..."/>` that still reference `NpcMovementLib` namespaces after the refactor.

### Step 2: Build all and commit

```bash
dotnet build NpcCommonLib/NpcCommonLib.csproj
dotnet build NpcMovementLib/NpcMovementLib.csproj
dotnet build NpcWeaponLib/NpcWeaponLib.csproj
dotnet build NpcTargetingLib/NpcTargetingLib.csproj
```

```bash
git add NpcCommonLib/ NpcTargetingLib/
git commit -m "docs: add comprehensive XML documentation to NpcTargetingLib and NpcCommonLib

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

---

## Task 8: READMEs

**Files:**
- Create: `NpcCommonLib/README.md`
- Create: `NpcTargetingLib/README.md`
- Modify: `NpcMovementLib/README.md` — update references from `NpcMovementLib.Math.Vec3` to `NpcCommonLib.Math.Vec3` etc.
- Modify: `NpcWeaponLib/README.md` — same namespace updates

### Step 1: Write NpcCommonLib README

Brief — it's a shared types library. Include:
- What it contains and why it exists
- Dependency diagram (Mermaid) showing all 4 libraries

### Step 2: Write NpcTargetingLib README

Follow the same structure as NpcMovementLib/README.md:
1. Title + overview
2. Features
3. Getting Started with usage example showing full pipeline (TargetingSimulator → MovementSimulator → FiringSimulator)
4. Component Design — Mermaid flowchart
5. Architecture — Mermaid class diagram
6. Target Selection Strategies
7. Threat Assessment
8. Lead Prediction
9. Integration with Movement + Weapon libs

### Step 3: Update existing READMEs

Update namespace references in NpcMovementLib/README.md and NpcWeaponLib/README.md.

### Step 4: Commit

```bash
git add NpcCommonLib/README.md NpcTargetingLib/README.md NpcMovementLib/README.md NpcWeaponLib/README.md
git commit -m "docs: add NpcTargetingLib and NpcCommonLib READMEs, update existing READMEs

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```

---

## Task 9: Code review

### Step 1: Cross-reference review

Use the code-reviewer agent to verify:
- `ThreatCalculator.GetHighestThreat()` matches `BehaviorContext.GetHighestThreatConstruct()` exactly (1-min window, group by construct, sum damage, fallback to closest)
- All three target strategies match their original Effect implementations
- `MovePositionCalculator` matches `CalculateTargetMovePositionWithOffsetEffect` (30s offset refresh, half-distance offset magnitude)
- `LeadPredictor.CalculatePredictionSeconds()` matches `BehaviorContext.CalculateMovementPredictionSeconds()` (10/30/60s tiers)
- NpcCommonLib refactor didn't break any builds
- No game-server dependencies leaked in
- No PrescriberPoint / healthcare references
- All 4 projects build with 0 warnings

### Step 2: Fix any issues and commit

```bash
git add .
git commit -m "fix: address code review findings

Co-Authored-By: Claude Opus 4.6 <noreply@anthropic.com>"
```
