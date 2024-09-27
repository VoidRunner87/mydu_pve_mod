using System;
using System.Collections.Generic;

namespace Mod.DynamicEncounters;

public static class StatsRecorder
{
    private class Stat
    {
        public long MinTime { get; private set; } = long.MaxValue;
        public long MaxTime { get; private set; } = long.MinValue;
        private readonly Queue<long> _times = new(50);

        // Method to add new time occurrence
        public void AddTime(long time)
        {
            if (_times.Count == 50) 
            {
                // Discard the oldest time
                _times.Dequeue();
            }

            // Add new time
            _times.Enqueue(time);

            // Update Min and Max
            MinTime = Math.Min(MinTime, time);
            MaxTime = Math.Max(MaxTime, time);
        }

        // Returns the 50 time occurrences
        public IEnumerable<long> GetTimes() => _times;
        
        public void Clear()
        {
            _times.Clear();
            MinTime = long.MaxValue;
            MaxTime = long.MinValue;
        }
    }

    // Static members to store stats for Movement and Targeting
    private static readonly Stat MovementStats = new();
    private static readonly Stat TargetingStats = new();

    // Methods to record movement stats
    public static void RecordMovement(long time)
    {
        MovementStats.AddTime(time);
    }

    // Methods to record targeting stats
    public static void RecordTargeting(long time)
    {
        TargetingStats.AddTime(time);
    }

    // Method to get movement statistics
    public static (long MinTime, long MaxTime, IEnumerable<long> Occurrences) GetMovementStats()
    {
        return (MovementStats.MinTime, MovementStats.MaxTime, MovementStats.GetTimes());
    }

    // Method to get targeting statistics
    public static (long MinTime, long MaxTime, IEnumerable<long> Occurrences) GetTargetingStats()
    {
        return (TargetingStats.MinTime, TargetingStats.MaxTime, TargetingStats.GetTimes());
    }

    public static void Clear()
    {
        MovementStats.Clear();
        TargetingStats.Clear();
    }
}