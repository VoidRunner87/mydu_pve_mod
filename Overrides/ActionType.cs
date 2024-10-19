namespace Mod.DynamicEncounters.Overrides;

public enum ActionType
{
    None = 0,
    NpcShootWeaponOnce = 1,
    LoadBoardApp = 1000000,
    RefreshNpcQuestList = 1000001,
    CloseBoard = 1000002,
    AcceptQuest = 1000003,
    LoadPlayerBoardApp = 1000004,
    RefreshPlayerQuestList = 1000005,
    AbandonQuest = 1000006,
    GiveTakePlayerItems = 100,
    Interact = 101,
    Callback = 1999999
}