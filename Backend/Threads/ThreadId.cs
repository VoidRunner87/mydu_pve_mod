namespace Mod.DynamicEncounters.Threads;

public enum ThreadId
{
    Caching = 1,
    Sector = 2,
    ExpirationNames = 3,
    BehaviorFeatureCheck = 4,
    ConstructHandleQuery = 5,
    ConstructBehaviorMedium = 6,
    ConstructBehaviorHigh = 7,
    ConstructBehaviorMovement = 8,
    TaskQueue = 9,
    Cleanup = 10,
}