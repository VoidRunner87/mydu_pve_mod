# Plan: Isolated NPC Movement Simulation Library

## Goal
Extract all NPC movement logic into a standalone C# class library (`NpcMovementLib`) that is **pure data-in / data-out** — no game server dependencies, no DI containers, no database, no NQ SDK types.

## Key Design Decisions

### Replace NQ SDK types with local structs
The original code depends on `NQ.Vec3` and `NQ.Quat` — external game SDK structs. The new project defines its own:
- `Vec3` (double x, y, z) with operator overloads (+, -, *, /)
- `Quat` (float x, y, z, w)

These will carry all the extension methods that currently live in `VectorMathHelper`, `VectorConversionHelpers`, `QuaternionConversionHelpers`, and `VectorMathUtils`.

### Input/Output Model
Instead of the existing `BehaviorContext` (which mixes movement state with combat, DI, skills, DB, etc.), we define clean input/output data classes:

**`MovementInput`** — everything the movement tick needs:
```
- Position (Vec3)                    // current NPC position
- Velocity (Vec3)                    // current NPC velocity
- Rotation (Quat)                    // current NPC rotation
- TargetMovePosition (Vec3)          // where to move toward
- DeltaTime (double)                 // seconds since last tick
- AccelerationG (double)             // acceleration in G units
- MaxSpeedKph (double)               // max speed in km/h
- MinSpeedKph (double)               // min speed in km/h
- RotationSpeed (float)              // slerp factor
- RealismFactor (double)             // blend: forward accel vs direct accel
- EnginePower (double)               // 0-1 engine power multiplier
- IsBraking (bool)                   // force braking
- Modifiers (VelocityModifiers)      // range-based velocity modifiers
- TargetDistance (double)             // distance to attack target (for velocity goal calc)
- TargetLinearVelocity (Vec3)        // target's velocity (for velocity goal calc)
- WeaponOptimalRange (double)        // weapon optimal range (for velocity goal calc)
- PreviousVelocity (Vec3?)           // optional: for BurnToTarget delta-V clamping
```

**`MovementOutput`** — the result:
```
- Position (Vec3)                    // new NPC position
- Velocity (Vec3)                    // new NPC velocity
- Rotation (Quat)                    // new NPC rotation
```

### Movement Strategies
The three movement effects become strategy implementations of a single interface:

```csharp
public interface IMovementStrategy
{
    MovementOutput Calculate(MovementInput input);
}
```

Implementations:
1. **`BurnToTargetStrategy`** — the default; uses `VelocityHelper.LinearInterpolateWithAccelerationV2` with velocity goal clamping and delta-V limiting
2. **`PIDMovementStrategy`** — PID controller-based movement with braking threshold
3. **`BrakingStrategy`** — decelerates to zero using per-axis braking

### Velocity Goal Calculator
The complex `CalculateVelocityGoal()` logic from `BehaviorContext` becomes a standalone pure function:

```csharp
public static class VelocityGoalCalculator
{
    public static double Calculate(VelocityGoalInput input) { ... }
}
```

### Waypoint Navigation
A simple stateful navigator, no DB/DI:

```csharp
public class WaypointNavigator
{
    public WaypointNavigator(IEnumerable<Vec3> waypoints, double arrivalDistance);
    public Vec3? GetCurrentTarget(Vec3 currentPosition);  // returns null when done
    public bool HasArrived { get; }
}
```

### Route Solver
Copy the nearest-neighbor TSP solver as a pure function:

```csharp
public static class RouteSolver
{
    public static IList<Vec3> Solve(Vec3 start, IEnumerable<Vec3> points);
}
```

---

## Project Structure

```
NpcMovementLib/
├── NpcMovementLib.csproj              (net8.0 class library, no external deps)
├── Math/
│   ├── Vec3.cs                        (struct + operator overloads + extension methods)
│   ├── Quat.cs                        (struct + conversions)
│   ├── VectorMathHelper.cs            (Size, ClampToSize, NormalizeSafe, Dot, Cross, Lerp, etc.)
│   ├── VectorMathUtils.cs             (SetRotationToMatchDirection, GetForward)
│   └── VelocityHelper.cs              (LinearInterpolate*, ApplyBraking, BrakingDistance, etc.)
├── Data/
│   ├── MovementInput.cs               (all inputs for a single tick)
│   ├── MovementOutput.cs              (position + velocity + rotation result)
│   ├── VelocityModifiers.cs           (range-based modifier config)
│   └── MovementConfig.cs              (AccelerationG, speeds, rotation, realism — prefab defaults)
├── Strategies/
│   ├── IMovementStrategy.cs           (interface)
│   ├── BurnToTargetStrategy.cs        (default movement)
│   ├── PIDMovementStrategy.cs         (PID-based movement)
│   ├── BrakingStrategy.cs             (deceleration)
│   └── PIDController.cs               (PID controller, copied)
├── Navigation/
│   ├── WaypointNavigator.cs           (stateful waypoint queue)
│   └── RouteSolver.cs                 (nearest-neighbor TSP)
├── VelocityGoalCalculator.cs          (pure function: distance/range → velocity goal)
└── MovementSimulator.cs               (orchestrator: combines strategy + rotation + velocity goal into one call)
```

## File-by-file Plan

### Step 1: Create project
- Create `NpcMovementLib/NpcMovementLib.csproj` — net8.0 class library, **zero external NuGet deps** (only `System.Numerics` from BCL)
- Add project to `Backend/Main.sln`

### Step 2: Math types (`Math/`)
- **`Vec3.cs`**: Define `public struct Vec3` with `double x, y, z`, operators `+`, `-`, `*` (scalar), `/` (scalar), `==`, `!=`. Include methods as instance/extension: `Size()`, `NormalizeSafe()`, `Normalized()`, `ClampToSize()`, `Dot()`, `CrossProduct()`, `Reverse()`, `Dist()`, `ToVector3()` (→ System.Numerics.Vector3), static `FromVector3()`. Ported from `VectorMathHelper.cs`.
- **`Quat.cs`**: Define `public struct Quat` with `float x, y, z, w`. Include `ToQuaternion()` (→ System.Numerics.Quaternion), static `FromQuaternion()`. Ported from `QuaternionConversionHelpers.cs`.
- **`VectorMathUtils.cs`**: Copy `SetRotationToMatchDirection()`, `GetForward()`, `ApplyRotation()` from `Backend/Helpers/VectorMathUtils.cs`. Adapt to use local `Vec3`/`Quat`.
- **`VelocityHelper.cs`**: Copy all methods from `Backend/Vector/Helpers/VelocityHelper.cs`. Adapt to use local `Vec3`.

### Step 3: Data classes (`Data/`)
- **`MovementConfig.cs`**: Mirrors the movement-relevant fields from `PrefabItem` (AccelerationG, RotationSpeed, MinSpeedKph, MaxSpeedKph, RealismFactor).
- **`VelocityModifiers.cs`**: Copy from `BehaviorModifiers.VelocityModifiers` + `ModifierByDotProduct`. Remove JSON attributes.
- **`MovementInput.cs`**: Single flat input class with all fields needed for one tick.
- **`MovementOutput.cs`**: Position + Velocity + Rotation.

### Step 4: Movement strategies (`Strategies/`)
- **`IMovementStrategy.cs`**: Interface with `MovementOutput Calculate(MovementInput input)`.
- **`BurnToTargetStrategy.cs`**: Port from `BurnToTargetMovementEffect.cs` + the acceleration/direction logic from `FollowTargetBehaviorV2.TickAsync()`. Pure function, no context/DI.
- **`PIDMovementStrategy.cs`**: Port from `PIDMovementEffect.cs`. Include rotation calculation.
- **`BrakingStrategy.cs`**: Port from `ApplyBrakesMovementEffect.cs`.
- **`PIDController.cs`**: Direct copy from `Backend/Features/Spawner/Behaviors/Services/PIDController.cs`, adapted to local `Vec3`.

### Step 5: Velocity goal calculator
- **`VelocityGoalCalculator.cs`**: Extract `CalculateVelocityGoal()` + `CalculateOverrideMoveVelocityGoal()` from `BehaviorContext.cs` into a static pure function. Input: distance, target velocity, min/max velocity, weapon optimal range, velocity modifiers, is-override flag.

### Step 6: Navigation (`Navigation/`)
- **`WaypointNavigator.cs`**: Stateful class with a `Queue<Vec3>`. Methods: `GetCurrentTarget(Vec3 currentPos)`, `ArriveIfClose()`, `Reset()`. No DB, no scripts.
- **`RouteSolver.cs`**: Port nearest-neighbor TSP from `TravelRouterService.cs`.

### Step 7: Orchestrator
- **`MovementSimulator.cs`**: Combines everything into a single `Tick(MovementInput) → MovementOutput` call:
  1. Calculate velocity goal via `VelocityGoalCalculator`
  2. Compute acceleration direction (forward + move blend via RealismFactor)
  3. Delegate to the selected `IMovementStrategy`
  4. Apply rotation via Slerp
  5. Return `MovementOutput`

This is the main entry point users call.

## What is NOT included (by design)
- No `IServiceProvider` / DI
- No database persistence
- No NQ SDK references (`NQ.Vec3`, `NQ.Quat`, `ConstructUpdate`, `BotLib`, etc.)
- No combat/weapon logic
- No radar/contact scanning
- No skills/script execution
- No threading/tick loop
- No `MathNet.Spatial` dependency — only `System.Numerics` from BCL
- No JSON serialization attributes
