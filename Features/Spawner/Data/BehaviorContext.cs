using System;
using System.Collections.Generic;
using BotLib.BotClient;
using NQ;

namespace Mod.DynamicEncounters.Features.Spawner.Data;

public class BehaviorContext(Client client, IServiceProvider serviceProvider)
{
    public delegate void CoreStressHighEvent(object sender, BehaviorEventArgs eventArgs);
    public delegate void ConstructDestroyedEvent(object sender, BehaviorEventArgs eventArgs);
    public delegate void ShieldHpHalfEvent(object sender, BehaviorEventArgs eventArgs);
    public delegate void ShieldHpLowEvent(object sender, BehaviorEventArgs eventArgs);
    public delegate void ShieldHpDownEvent(object sender, BehaviorEventArgs eventArgs);

    public event CoreStressHighEvent OnCoreStressHigh;
    public event ConstructDestroyedEvent OnConstructDestroyed;
    public event ShieldHpHalfEvent OnShieldHpHalf;
    public event ShieldHpLowEvent OnShieldHpLow;
    public event ShieldHpDownEvent OnShieldHpDown;
    
    public ulong? TargetConstructId { get; set; }
    public double DeltaTime { get; set; }
    public Vec3 Velocity { get; set; }
    public HashSet<ulong> PlayerIds { get; set; }
    public IServiceProvider ServiceProvider { get; init; } = serviceProvider;
    public Client Client { get; set; } = client;

    public HashSet<string> PublishedEvents = [];
    
    public DateTime? TargetSelectedTime { get; set; }
    
    public bool IsAlive { get; set; }
    
    public bool IsActiveWreck { get; set; }

    public virtual void NotifyCoreStressHigh(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.Contains(nameof(OnCoreStressHigh)))
        {
            return;
        }
            
        OnCoreStressHigh?.Invoke(this, eventArgs);
        PublishedEvents.Add(nameof(OnCoreStressHigh));
    }

    public virtual void NotifyConstructDestroyed(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.Contains(nameof(OnConstructDestroyed)))
        {
            return;
        }
        
        OnConstructDestroyed?.Invoke(this, eventArgs);
        PublishedEvents.Add(nameof(OnConstructDestroyed));
    }
    
    public virtual void NotifyShieldHpHalf(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.Contains(nameof(OnShieldHpHalf)))
        {
            return;
        }
        
        OnShieldHpHalf?.Invoke(this, eventArgs);
        PublishedEvents.Add(nameof(OnShieldHpHalf));
    }
    
    public virtual void NotifyShieldHpLow(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.Contains(nameof(OnShieldHpLow)))
        {
            return;
        }
        
        OnShieldHpLow?.Invoke(this, eventArgs);
        PublishedEvents.Add(nameof(OnShieldHpLow));
    }
    
    public virtual void NotifyShieldHpDown(BehaviorEventArgs eventArgs)
    {
        if (PublishedEvents.Contains(nameof(OnShieldHpDown)))
        {
            return;
        }
        
        OnShieldHpDown?.Invoke(this, eventArgs);
        PublishedEvents.Add(nameof(OnShieldHpDown));
    }
}