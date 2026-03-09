using System.Numerics;
using NpcCommonLib.Data;
using NpcCommonLib.Math;
using NpcMovementLib;
using NpcMovementLib.Data;
using NpcTargetingLib;
using NpcTargetingLib.Data;
using NpcWeaponLib;
using NpcWeaponLib.Data;
using Xunit;
using Xunit.Abstractions;

namespace NpcSimulation.Tests;

public class NpcCombatSimulationTest
{
    private readonly ITestOutputHelper _output;

    public NpcCombatSimulationTest(ITestOutputHelper output)
    {
        _output = output;
    }

    // ── Mock Data Factories ────────────────────────────────────────

    /// <summary>Creates a small railgun with 50 km optimal range, 20 km falloff.</summary>
    private static WeaponStats CreateSmallRailgun() => new()
    {
        ItemTypeId = 1001,
        ItemTypeName = "WeaponRailgunSmallPrecision3",
        DisplayName = "Rare Small Railgun s",
        BaseDamage = 3500,
        BaseAccuracy = 0.9,
        BaseOptimalDistance = 50_000,     // 50 km optimal
        FalloffDistance = 20_000,         // 20 km falloff
        BaseOptimalTracking = 1.0,
        FalloffTracking = 0.5,
        BaseOptimalAimingCone = 3.0,
        FalloffAimingCone = 1.5,
        OptimalCrossSectionDiameter = 32,
        BaseCycleTime = 3.0,             // 3s between shots in magazine
        BaseReloadTime = 8.0,            // 8s reload
        MagazineVolume = 100,            // 100L magazine
        AmmoItems =
        [
            new AmmoStats
            {
                ItemTypeId = 2001,
                ItemTypeName = "AmmoRailgunSmallKineticAdvanced",
                Scale = "s",
                Level = 3,
                DamageType = "kinetic",
                UnitVolume = 12.5,       // 100L * 1.5 mag buff / 12.5 = 12 shots per mag
            }
        ],
    };

    /// <summary>Creates weapon effectiveness for a fully healthy weapon group.</summary>
    private static IDictionary<string, IList<WeaponEffectiveness>> CreateHealthyWeapons(
        string weaponTypeName, int count)
    {
        var list = new List<WeaponEffectiveness>();
        for (var i = 0; i < count; i++)
        {
            list.Add(new WeaponEffectiveness { Name = weaponTypeName, HitPointsRatio = 1.0 });
        }
        return new Dictionary<string, IList<WeaponEffectiveness>> { [weaponTypeName] = list };
    }

    /// <summary>Creates a radar contact for the player at the given position.</summary>
    private static ScanContact CreatePlayerContact(Vec3 npcPosition, Vec3 playerPosition) => new()
    {
        ConstructId = new ConstructId(9999),
        Name = "Player Ship",
        Distance = npcPosition.Dist(playerPosition),
        Position = playerPosition,
    };

    // ── The Simulation ─────────────────────────────────────────────

    [Fact]
    public void Npc_DetectsPlayer_BurnsToward_AndFires()
    {
        // --- Config ---
        const double deltaTime = 0.05;            // 20 FPS
        const int maxFrames = 60_000;              // 50 min max (safety cap)
        const double npcAccelG = 20;               // 20g thrust
        const double maxSpeedKph = 20_000;         // 20,000 km/h max
        const double minSpeedKph = 2_000;          // 2,000 km/h min combat speed
        const double weaponOptimalRange = 50_000;  // 50 km
        const double maxEngagementRange = 400_000; // 400 km (2 SU)

        var npcId = new ConstructId(1);

        // NPC starts at origin, player is 200 km away on the X axis
        var npcPosition = new Vec3(0, 0, 0);
        var playerPosition = new Vec3(200_000, 0, 0);
        var npcVelocity = new Vec3(0, 0, 0);
        var npcRotation = Quaternion.Identity;

        var weapon = CreateSmallRailgun();
        var weaponEffectiveness = CreateHealthyWeapons(weapon.ItemTypeName, 4); // 4 railguns

        // --- Simulators ---
        var targetingSim = new TargetingSimulator(); // default: HighestThreat
        var movementSim = new MovementSimulator();   // default: BurnToTarget
        var firingSim = new FiringSimulator();

        // --- Tracking ---
        var firstTargetAcquiredFrame = -1;
        var firstFiredFrame = -1;
        var firstInRangeFiredFrame = -1;
        var closestDistance = double.MaxValue;
        var maxSpeed = 0.0;
        Vec3? previousVelocity = null;

        _output.WriteLine($"{"Frame",6} {"Time(s)",8} {"Dist(km)",10} {"Speed(km/h)",12} {"HasTarget",10} {"Fired",6} {"Reason",20}");
        _output.WriteLine(new string('-', 80));

        for (var frame = 0; frame < maxFrames; frame++)
        {
            var distanceToPlayer = npcPosition.Dist(playerPosition);

            // Update contact distance each frame
            var contacts = new List<ScanContact>
            {
                CreatePlayerContact(npcPosition, playerPosition)
            };

            // ── 1. TARGETING ──
            var targetingInput = new TargetingInput
            {
                ConstructId = npcId,
                Position = npcPosition,
                StartPosition = new Vec3(0, 0, 0), // home position
                Contacts = contacts,
                DeltaTime = deltaTime,
                WeaponOptimalRange = weaponOptimalRange,
                TargetLinearVelocity = new Vec3(0, 0, 0), // player is stationary
                TargetAcceleration = new Vec3(0, 0, 0),
            };

            var targetingOutput = targetingSim.Tick(targetingInput);

            if (targetingOutput.HasTarget && firstTargetAcquiredFrame < 0)
                firstTargetAcquiredFrame = frame;

            // ── 2. MOVEMENT ──
            var movementInput = new MovementInput
            {
                Position = npcPosition,
                Velocity = npcVelocity,
                Rotation = npcRotation,
                TargetMovePosition = targetingOutput.MoveToPosition,
                DeltaTime = deltaTime,
                AccelerationG = npcAccelG,
                MaxSpeedKph = maxSpeedKph,
                MinSpeedKph = minSpeedKph,
                RotationSpeed = 1.0f,   // fast rotation (NPC can turn quickly)
                RealismFactor = 0.0,    // arcade steering (direct to target)
                EnginePower = 1.0,
                IsBraking = false,
                TargetDistance = distanceToPlayer,
                WeaponOptimalRange = weaponOptimalRange,
                PreviousVelocity = previousVelocity,
            };

            var movementOutput = movementSim.Tick(movementInput);

            // ── 3. FIRING ──
            var firingInput = new FiringInput
            {
                ConstructId = npcId,
                Position = movementOutput.Position,
                ConstructSize = 64,
                IsAlive = true,
                TargetConstructId = targetingOutput.TargetConstructId,
                TargetPosition = targetingOutput.HasTarget ? targetingOutput.TargetPosition : playerPosition,
                Weapons = [weapon],
                WeaponEffectiveness = weaponEffectiveness,
                Modifiers = new WeaponModifiers(),
                AmmoTier = 3,
                AmmoVariant = "Kinetic",
                DeltaTime = deltaTime,
                MaxWeaponCount = 4,
                MaxEngagementRange = maxEngagementRange,
            };

            var firingOutput = firingSim.Tick(firingInput);

            if (firingOutput.ShouldFire && firstFiredFrame < 0)
                firstFiredFrame = frame;

            if (firingOutput.ShouldFire && firstInRangeFiredFrame < 0 &&
                movementOutput.Position.Dist(playerPosition) <= weaponOptimalRange)
                firstInRangeFiredFrame = frame;

            // ── 4. FEED BACK ──
            previousVelocity = npcVelocity;
            npcPosition = movementOutput.Position;
            npcVelocity = movementOutput.Velocity;
            npcRotation = movementOutput.Rotation;

            // Track stats
            var currentDistance = npcPosition.Dist(playerPosition);
            var currentSpeedKph = npcVelocity.Size() * 3.6;
            closestDistance = Math.Min(closestDistance, currentDistance);
            maxSpeed = Math.Max(maxSpeed, currentSpeedKph);

            // Log every 100 frames (5 seconds) + first few frames + fire events
            var simTime = frame * deltaTime;
            if (frame < 5 || frame % 100 == 0 || firingOutput.ShouldFire)
            {
                var reason = firingOutput.SuppressedReason?.ToString() ?? (firingOutput.ShouldFire ? "FIRED!" : "");
                _output.WriteLine(
                    $"{frame,6} {simTime,8:F2} {currentDistance / 1000,10:F1} {currentSpeedKph,12:F0} " +
                    $"{targetingOutput.HasTarget,10} {firingOutput.ShouldFire,6} {reason,20}");
            }

            // Stop after first in-range shot + 20 frames for a clean test
            if (firstInRangeFiredFrame >= 0 && frame > firstInRangeFiredFrame + 20)
                break;
        }

        // ── ASSERTIONS ──
        _output.WriteLine(new string('=', 80));
        _output.WriteLine($"Target acquired on frame: {firstTargetAcquiredFrame} ({firstTargetAcquiredFrame * deltaTime:F2}s)");
        _output.WriteLine($"First shot fired on frame: {firstFiredFrame} ({firstFiredFrame * deltaTime:F2}s)");
        _output.WriteLine($"First in-range shot on frame: {firstInRangeFiredFrame} ({firstInRangeFiredFrame * deltaTime:F2}s)");
        _output.WriteLine($"Closest distance: {closestDistance / 1000:F1} km");
        _output.WriteLine($"Max speed reached: {maxSpeed:F0} km/h");

        // Target should be acquired on the very first frame (player is on radar)
        Assert.True(firstTargetAcquiredFrame == 0, "NPC should acquire target on frame 0");

        // NPC should eventually fire (within the simulation window)
        Assert.True(firstFiredFrame > 0, "NPC should fire at least once");

        // NPC should have closed distance significantly from 200 km
        Assert.True(closestDistance < weaponOptimalRange,
            $"NPC should close to within weapon range ({weaponOptimalRange / 1000} km), " +
            $"but closest was {closestDistance / 1000:F1} km");

        // NPC should fire at least once while within optimal range
        Assert.True(firstInRangeFiredFrame > 0,
            "NPC should fire at least once while within optimal range");

        // Max speed should be reasonable (accelerated to combat speed)
        Assert.True(maxSpeed > minSpeedKph,
            $"NPC should exceed min combat speed ({minSpeedKph} km/h), " +
            $"but max was {maxSpeed:F0} km/h");
    }
}
