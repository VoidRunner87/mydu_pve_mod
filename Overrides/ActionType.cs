namespace Mod.DynamicEncounters.Overrides;

public enum ActionType
{
    None = 0,
    NpcShootWeaponOnce = 1,
    LoadNpcApp = 1000000,
    RefreshNpcApp = 1000001,
    CloseNpcApp = 1000002,
    AcceptQuest = 1000003,
    Callback = 1999999
}