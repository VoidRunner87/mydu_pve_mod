using System;
using System.Threading;
using System.Threading.Tasks;

namespace NpcMovementLib;

/// <summary>
/// A self-contained example of the fixed-step update loop technique used to drive NPC movement.
///
/// WHY FIXED STEP?
/// ===============
/// In a variable-timestep loop, delta time changes every frame depending on how long the previous
/// frame took. This leads to inconsistent physics: an NPC might overshoot a target on a slow frame
/// or jitter on a fast one. A fixed-step loop guarantees that every simulation tick uses the exact
/// same delta time (e.g. 50ms), producing deterministic, reproducible movement regardless of
/// real-world timing.
///
/// HOW IT WORKS
/// ============
/// 1. Each iteration, we measure how much real time has elapsed since the last iteration.
/// 2. That elapsed time is added to an accumulator.
/// 3. We then consume the accumulator in fixed-size chunks (the fixed delta time).
///    - If the machine is fast, the accumulator may be less than one chunk → no tick this iteration.
///    - If the machine is slow or a frame spike occurs, the accumulator may hold several chunks
///      → we run multiple "catch-up" ticks to keep the simulation in sync with real time.
/// 4. A maximum catch-up limit (MaxFixedStepLoops) prevents a death spiral: if the simulation
///    falls too far behind (e.g. the process was suspended), we cap the catch-up and reset
///    the accumulator rather than running hundreds of ticks at once.
///
/// VARIABLE-STEP MODE
/// ==================
/// Also included for comparison. In this mode, delta time is simply the real elapsed time
/// since the last tick. Useful for logic that doesn't need deterministic timing (e.g. target
/// selection, AI decision-making at 1 FPS).
///
/// USAGE IN THE MOD
/// ================
/// The mod runs three loops at different priorities:
///   - MovementPriority:  20 FPS, fixed step (this technique) → position/velocity updates
///   - HighPriority:      10 FPS, variable step              → combat, damage, targeting
///   - MediumPriority:     1 FPS, variable step              → target selection, AI decisions
///
/// Each loop processes all active NPC constructs in parallel via Parallel.ForEachAsync,
/// and each NPC behavior declares which loop category it belongs to.
/// </summary>
public class FixedStepUpdateLoop
{
    // -- Configuration --

    /// <summary>
    /// The fixed delta time for each simulation step: 1/20th of a second = 50ms.
    /// Every tick callback receives exactly this value, ensuring deterministic physics.
    /// </summary>
    private const double FixedDeltaTime = 1.0 / 20.0; // 50ms

    /// <summary>
    /// Maximum number of fixed-step ticks to run in a single iteration.
    /// Prevents a "death spiral" where the simulation can never catch up
    /// if real time gets too far ahead (e.g. after a process freeze or long GC pause).
    /// If we exceed this, the accumulator is reset to zero — we accept losing
    /// some simulation time rather than freezing the server with hundreds of catch-up ticks.
    /// </summary>
    private const int MaxFixedStepLoops = 10;

    /// <summary>
    /// Target frames per second. Controls how long we sleep between iterations
    /// to avoid busy-spinning. The actual tick rate is governed by the fixed step,
    /// but this prevents the loop from running thousands of idle iterations per second.
    /// </summary>
    private readonly double _targetFps;

    /// <summary>
    /// When true, uses fixed-step mode (accumulator + catch-up).
    /// When false, uses variable-step mode (real delta time passed directly).
    /// </summary>
    private readonly bool _useFixedStep;

    // -- State --
    private DateTime _lastTickTime;
    private TimeSpan _accumulatedTime = TimeSpan.Zero;

    public FixedStepUpdateLoop(double targetFps = 20, bool useFixedStep = true)
    {
        if (targetFps <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetFps), "Must be > 0");

        _targetFps = targetFps;
        _useFixedStep = useFixedStep;
        _lastTickTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Runs the update loop until cancellation is requested.
    /// The provided callback is invoked each tick with a fixed or variable delta time.
    ///
    /// Example usage:
    /// <code>
    ///   var loop = new FixedStepUpdateLoop(targetFps: 20, useFixedStep: true);
    ///   await loop.RunAsync(async (deltaTime, ct) =>
    ///   {
    ///       // deltaTime is always exactly 0.05 (50ms) in fixed-step mode
    ///       npc.Position += npc.Velocity * deltaTime;
    ///       npc.Velocity += npc.Acceleration * deltaTime;
    ///       await SendPositionUpdate(npc);
    ///   }, cancellationToken);
    /// </code>
    /// </summary>
    public async Task RunAsync(
        Func<double, CancellationToken, Task> onTick,
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_useFixedStep)
                    await RunFixedStepIteration(onTick, stoppingToken);
                else
                    await RunVariableStepIteration(onTick, stoppingToken);

                // Yield control so other async work can proceed.
                // Without this, the loop would starve other tasks on the same thread.
                await Task.Yield();
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// VARIABLE-STEP MODE
    ///
    /// Simple approach: measure real elapsed time, sleep if we're ahead of target FPS,
    /// then tick once with the actual delta time.
    ///
    /// Pros: Simple, no accumulator logic.
    /// Cons: Delta time varies, so physics can be inconsistent on slow/fast frames.
    ///
    /// Used for: AI decisions, target selection — things that don't need frame-perfect timing.
    /// </summary>
    private async Task RunVariableStepIteration(
        Func<double, CancellationToken, Task> onTick,
        CancellationToken stoppingToken)
    {
        // Step 1: Measure real elapsed time
        var now = DateTime.UtcNow;
        var deltaTime = now - _lastTickTime;
        _lastTickTime = now;

        // Step 2: Sleep if we're running faster than our target FPS
        var targetFrameTime = 1.0 / _targetFps;
        if (deltaTime.TotalSeconds < targetFrameTime)
        {
            var waitSeconds = targetFrameTime - deltaTime.TotalSeconds;
            await Task.Delay(TimeSpan.FromSeconds(waitSeconds), stoppingToken);
        }

        // Step 3: Tick once with the real delta time
        await onTick(deltaTime.TotalSeconds, stoppingToken);
    }

    /// <summary>
    /// FIXED-STEP MODE (the important one for movement)
    ///
    /// This is the core technique. Instead of passing variable delta time to the tick,
    /// we accumulate real elapsed time and consume it in fixed-size chunks.
    ///
    /// Timeline example (target 20 FPS = 50ms per tick):
    ///
    ///   Real time:     |----62ms----|----45ms----|----130ms----|----30ms----|
    ///   Accumulator:   62ms         57ms         137ms         17ms
    ///   Ticks fired:   1 tick       1 tick       2 ticks       0 ticks
    ///                  (12ms left)  (7ms left)   (37ms left)   (17ms left → carries over)
    ///
    /// The leftover accumulator carries forward, so over time the simulation
    /// stays perfectly in sync with real time without drift.
    ///
    /// CATCH-UP SCENARIO:
    ///   If the process freezes for 2 seconds, the accumulator would be 2000ms.
    ///   At 50ms per tick, that's 40 ticks — too many. MaxFixedStepLoops (10) caps it.
    ///   After 10 catch-up ticks, the accumulator is reset to zero.
    ///   We lose ~1.5 seconds of simulation time, but the server stays responsive.
    /// </summary>
    private async Task RunFixedStepIteration(
        Func<double, CancellationToken, Task> onTick,
        CancellationToken stoppingToken)
    {
        // Step 1: Measure real elapsed time since last iteration
        var now = DateTime.UtcNow;
        var deltaTime = now - _lastTickTime;
        _lastTickTime = now;

        // Step 2: Sleep if we're running faster than target FPS
        //         This is just to avoid busy-spinning. The fixed step handles the rest.
        var targetFrameTime = 1.0 / _targetFps;
        if (deltaTime.TotalSeconds < targetFrameTime)
        {
            var waitSeconds = targetFrameTime - deltaTime.TotalSeconds;
            await Task.Delay(TimeSpan.FromSeconds(waitSeconds), stoppingToken);
        }

        // Step 3: Add elapsed real time to the accumulator
        _accumulatedTime += deltaTime;

        var fixedDeltaSpan = TimeSpan.FromSeconds(FixedDeltaTime);
        var tickCount = 0;

        // Step 4: Consume the accumulator in fixed-size chunks
        //         Each tick gets exactly FixedDeltaTime (50ms), guaranteeing determinism.
        while (_accumulatedTime >= fixedDeltaSpan)
        {
            if (stoppingToken.IsCancellationRequested) return;

            // Fire one tick with the fixed delta time (always 0.05 seconds)
            await onTick(FixedDeltaTime, stoppingToken);

            // Subtract the chunk we just consumed
            _accumulatedTime -= fixedDeltaSpan;

            tickCount++;

            // Step 5: Catch-up safety valve
            //         If we've run too many ticks this iteration, bail out.
            //         This prevents a "death spiral" where catch-up ticks take so long
            //         that even more time accumulates, leading to even more catch-up ticks.
            if (tickCount > MaxFixedStepLoops)
            {
                // Discard remaining accumulated time — accept the loss
                _accumulatedTime = TimeSpan.Zero;
                break;
            }
        }
    }
}
