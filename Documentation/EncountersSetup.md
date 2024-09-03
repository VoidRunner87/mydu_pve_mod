# Setting up Encounters

# Enable Spawner Features

In the `mod_features` table, enable (set to "true"):
* TaskQueueLoopEnabled
* HealthCheckLoopEnabled
* SectorLoopEnabled

Set the minutes of expiration - 360 should do it
This is deprecated
* ConstructBehaviorLoopEnabled

How many sectors to generate - choose that based on how powerful your server is
* SectorsToGenerate - at least 10 is what I'm using on a decent server setup

## Create Construct Definitions (or Prefabs)

Prefabs will have the buffs/debuffs of what blueprint you want to spawn.
Also it will set the name of that construct and scripts to run when events happen.

### Properties

| Property | Required | Description |
| --- | --- | --- |
| Mods.Weapon | No | Defaults all to 1. A value that is multiplied to the weapon stats to give the NPC more or less capability. |
| Name | Yes | Name of script. It also has to be the same on the name column |
| Path | Yes | Blueprint file to use - not tested with deeper folder paths |
| Folder | Yes | Folder name where the BP file is - not tested with nested folders |
| IsNpc | Yes | `true` or `false`. True makes the construct show up with a yellow marker. It caused bugs on my tests when the construct was cored. Recommend false for now |
| Events | No | Events to execute scripts on - more on that later |
| OwnerId | Yes | Recommend 2 or 4. 2 = Aphelia; 4 = unknown |
| AmmoItems | No | If you don't set anything the damage is going to be the same as a missile weapon. Look on the BO what kind of ammo to use. The buffs are respected |
| WeaponItems | No | If you don't set anything the damage is going to be the same as a missile weapon. Look on the BO what kind of ammo to use. The buffs are respected |
| IsUntargetable | No | Untested |
| InitialBehaviors | No | If none, the construct does nothing. `aggressive` and `follow-target` are needed for ships to fight |
| ServerProperties.Header.PrettyName | Yes | Construct Name |
| IsDynamicWreck | No | Default is False. Makes a construct appear in the Points of Interest as a Wreck. |

### Example

```json
{
  "Mods": {
    "Weapon": {
      "Damage": 1,
      "Accuracy": 1,
      "CycleTime": 1,
      "FalloffDistance": 1,
      "FalloffTracking": 1,
      "OptimalDistance": 2,
      "OptimalTracking": 1,
      "FalloffAimingCone": 1,
      "OptimalAimingCone": 1
    }
  },
  "Name": "easy-pirate-rogue-1",
  "Path": "Pirate_Rogue.json",
  "IsNpc": false,
  "Events": {
    "OnShieldLow": [
      {
        "Type": "message",
        "Message": "Our shields are weak!"
      }
    ],
    "OnShieldDown": [
      {
        "Type": "message",
        "Message": "Our shields are down!"
      }
    ],
    "OnShieldHalf": [
      {
        "Type": "random",
        "Actions": [
          {
            "Type": "message",
            "Message": "Our shields are taking a beating!"
          },
          {
            "Type": "message",
            "Message": "You will pay for this!"
          }
        ]
      }
    ],
    "OnDestruction": [
      {
        "Type": "message",
        "Message": "AAAHAAAHHH!"
      }
    ],
    "OnCoreStressHigh": [
      {
        "Type": "message",
        "Message": "Our reactor is overloading!"
      }
    ]
  },
  "Folder": "pve",
  "OwnerId": 4,
  "AmmoItems": [
    "AmmoCannonSmallKineticAdvancedPrecision",
    "AmmoCannonSmallThermicAdvancedPrecision"
  ],
  "WeaponItems": [
    "WeaponCannonSmallPrecision3"
  ],
  "IsUntargetable": false,
  "InitialBehaviors": [
    "aggressive",
    "follow-target"
  ],
  "ServerProperties": {
    "Header": {
      "PrettyName": "Rogue Pirate"
    },
    "IsDynamicWreck": false
  }
}
```

