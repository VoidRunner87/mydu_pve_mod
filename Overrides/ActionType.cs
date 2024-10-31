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
    SendConstructAppear = 1000007,
    GiveTakePlayerItems = 100,
    Interact = 101,
    FetchPlayerParty = 102,
    LoadPlayerParty = 103,
    SetPlayerLocation = 104,
    LeaveParty = 105,
    DisbandParty = 106,
    CancelPartyInvite = 107,
    AcceptPartyRequest = 108,
    RejectPartyRequest = 109,
    SetPartyRole = 110,
    InviteToParty = 111,
    CreateParty = 112,
    AcceptInvite = 113,
    Callback = 1999999
}