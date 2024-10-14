using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Mod.DynamicEncounters.Features.Spawner.Behaviors.Interfaces;

namespace Mod.DynamicEncounters;

public static class StatsRecorder
{
    public class Stat
    {
        public long MinTime { get; private set; } = long.MaxValue;
        public long MaxTime { get; private set; } = long.MinValue;
        public double Average => _times.Average();
        
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
    private static readonly ConcurrentDictionary<BehaviorTaskCategory, Stat> Stats = new();
    private static readonly ConcurrentDictionary<string, Stat> CustomStats = new();

    // Methods to record movement stats
    public static void Record(BehaviorTaskCategory category, long time)
    {
        Stats.AddOrUpdate(
            category,
            _ =>
            {
                var stat = new Stat();
                stat.AddTime(time);
                return stat;
            },
            (taskCategory, stat) =>
            {
                stat.AddTime(time);
                return stat;
            });
    }
    
    public static void Record(string name, long time)
    {
        CustomStats.AddOrUpdate(
            name,
            _ =>
            {
                var stat = new Stat();
                stat.AddTime(time);
                return stat;
            },
            (taskCategory, stat) =>
            {
                stat.AddTime(time);
                return stat;
            });
    }

    public static ConcurrentDictionary<BehaviorTaskCategory, Stat> GetStats() => Stats;
    public static ConcurrentDictionary<string, Stat> GetCustomStats() => CustomStats;
    
    public static void ClearAll()
    {
        foreach (var kvp in Stats)
        {
            kvp.Value.Clear();
        }

        foreach (var kvp in CustomStats)
        {
            kvp.Value.Clear();
        }
    }
}