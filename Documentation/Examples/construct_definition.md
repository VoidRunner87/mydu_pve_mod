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
