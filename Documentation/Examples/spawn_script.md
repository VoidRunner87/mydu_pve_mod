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
      "Tags": [
        "poi"
      ],
      "Type": "for-each-handle-with-tag",
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
