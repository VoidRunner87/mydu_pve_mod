# Setting up Encounters

The use case this document covers is a POI showing on Points of Interest with a `[1] Pirate Attack` marker.
When a player enters the sector area (10su radius - currently hardcoded in the code - may change in the future) it spawns a pirate ship to attack the player.

# Recommendations

I recommend you DISABLE the Vanilla Wreck system.

To disable the vanilla wreck system:
Go to the backoffice -> Item Hierarchy -> search FeaturesList -> Open it -> set `dynamicWrecks` to `false` and save

Reasons:

* The ships available are bugged (Specially the Unknown Origins L Core)
* This mod leverages the POI list for people to find encounters. Having a TONS of wrecks is going to make that more difficult.
* You can spawn a wreck pool instead of constantly spawning wrecks using this mod.

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
| InitialBehaviors | No | If none, the construct does nothing ("wreck" behavior). `aggressive` and `follow-target` are needed for ships to fight |
| ServerProperties.Header.PrettyName | Yes | Construct Name |
| IsDynamicWreck | No | Default is False. Makes a construct appear in the Points of Interest as a Wreck. |

### DB Table

`mod_construct_def`

### Example Aggressive Construct

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

### Example POI

Table `mod_script`

```json
{
  "Mods": {
    "Weapon": {
      "Damage": 1,
      "Accuracy": 1,
      "CycleTime": 1,
      "FalloffDistance": 1,
      "FalloffTracking": 1,
      "OptimalDistance": 1,
      "OptimalTracking": 1,
      "FalloffAimingCone": 1,
      "OptimalAimingCone": 1
    }
  },
  "Name": "wreck-medium-pirate-attack-1",
  "Path": "Wreck_4_Shade.json",
  "IsNpc": false,
  "Events": {
    "OnShieldLow": [],
    "OnShieldDown": [],
    "OnShieldHalf": [],
    "OnDestruction": [],
    "OnCoreStressHigh": []
  },
  "Folder": "wrecks",
  "OwnerId": 0,
  "IsUntargetable": false,
  "InitialBehaviors": [
    "wreck"
  ],
  "ServerProperties": {
    "Header": {
      "PrettyName": "[1] Pirate Raid"
    },
    "IsDynamicWreck": true
  }
}
```

## Create Spawner Scripts

### POI

Table `mod_script`

This script will spawn a Wreck.
It does nothing else than spawning a wreck. Type=spawn, Prefab=wreck-medium-pirate-attack-1 prefab matches the field `name` of table `mod_construct_def`

```json
{
  "Area": {
    "Type": "sphere",
    "Radius": 200000
  },
  "Name": "spawn-poi-easy-pirate-attack-1",
  "Tags": [
    "poi"
  ],
  "Type": "spawn",
  "Events": {
    "OnLoad": [],
    "OnSectorEnter": []
  },
  "Prefab": "wreck-medium-pirate-attack-1",
  "Script": null,
  "Actions": [],
  "Message": null,
  "Position": null,
  "ConstructId": 0,
  "MaxQuantity": 1,
  "MinQuantity": 1
}
```

### Pirate Ship

Table `mod_script`

As seen before in this document, it spawns a pirate ship:
* The pirate ship has Advanced Cannons Small Agile and fires T3 Ammo for Cannons
* The ship has twice the distance of the weapon setup - to make it more effective against a player
* The ship sends chat DM to players when Shields are down, half, low and CCS is high and finally when they get cored - notice the Type="message" on the script for the events
* The value for the `name` column of the table `mod_construct_def` is going to be the same as the "Name" json property

Below we are spawning that Prefab. The script does:
* Spawns the prefab around an area of 100000 meters (100km/0.5su) around the sector's coords
* After it's spawned it executes a script called: `for-each-handle-with-tag` which removes the POI on that sector.
  * I did that because I want to avoid the POI section to have trash and encounters already visited. Made the experience better.
* Expires the sector. It immediately expires the sector. HOWEVER, since there's a player inside it, it will postpone expiration to 30min from the current date time.
  * Useful again to allow the pool to refresh immediately and spawn a new sector once the player leaves the area.
  * Also there's a forced expiration if someone decides to make camp there and logout - DON'T do that... people will murder you lol.
* Last one is not working because the code doesn't know who exactly is in the sector... but the idea is to send a Not-so-welcome DM for RP/interesting purposes.

```json
{
  "Name": "easy-pirate-rogue-1",
  "Actions": [
    {
      "Area": {
        "Type": "sphere",
        "Radius": 100000
      },
      "Name": "easy-pirate-rogue-1",
      "Type": "spawn",
      "Events": {
        "OnLoad": [],
        "OnSectorEnter": []
      },
      "Prefab": "easy-pirate-rogue-1",
      "Script": null,
      "Actions": [],
      "Message": null,
      "Position": null,
      "ConstructId": 0,
      "MaxQuantity": 1,
      "MinQuantity": 1
    },
    {
      "Type": "for-each-handle-with-tag",
      "Tags": [
        "poi"
      ],
      "Actions": [
        {
          "Type": "remove-poi"
        }
      ]
    },
    {
      "Type": "expire-sector"
    },
    {
      "Type": "random",
      "Actions": [
        {
          "Type": "message",
          "Message": "Looks who's come to play!"
        },
        {
          "Type": "message",
          "Message": "I have you now!"
        },
        {
          "Type": "message",
          "Message": "Fresh meat on radar!"
        }
      ]
    }
  ]
}
```

## Setup the Encounters Pool

DB Table = `mod_sector_encounter`
Fields:
* on_load_script - Executes when the encounter is loaded - AVOID spawning things that have behaviors here. It can overload your CPU
* on_sector_enter_script - this executes when a player enters the sector - Used to spawn the actual encounter

Insert `one` record on the table `mod_sector_encounter` for `easy-pirate-rogue-1`. Fields:
* name = `Easy Pirate Attack 1`
* on_load_script = `spawn-poi-easy-pirate-attack-1`
* on_sector_enter_script = 1easy-pirate-rogue-1`
* active = `true`

## Restart the Container

There's no consequence to restarting the container, except that if someone is fighting something the NPC will stop.
Once the container is restarted, the NPC continues the fight where it stopped.

## What we've done

* Created construct definitions for POI and a Pirate Ship
* Created scripts to spawn the POI and the Pirate Ship
* Created an entry on `mod_sector_encounter` table to serve as one of many encounters

## What happens next

* The PVE mod is going to check the sector_instance table and see if there are any sectors to expire - then it will expire and delete them.
* The PVE mod is going to randomly pick encounters to spawn and add entries to tables: `mod_sector_instance` and `mod_npc_construct_handle` to track things it creates.
  * The sector manager is going to execute the `on_loaded_script` on each sector added
* The PVE mod is going to query the spatial hash (sort of) of constructs until it finds a match in a sector instance and execute the script `on_sector_entered_script`
* Any aggressive ships spawned will hunt the player
* Every 2 seconds or so the ship looks for targets and shoots, moves and tracks players
* NPC ships are not currently damaging voxels of player ships... NOT configurable YET.


