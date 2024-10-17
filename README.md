# PVE Mod for MyDU

[![image](https://github.com/user-attachments/assets/8d5a4b86-d3a2-4319-b715-9a5608dbb6bc)](https://www.youtube.com/watch?v=vlXTiFBxXbk)

Before starting, know this:
* There were several hacky adjustments I had to make for this to work.
  * UPDATE: Some of those were cleaned up
* I first focused on getting the solution ready to run as a separate docker container.
* After the basic functionality I envision for this mod is done, I'll start looking into optimizing the cycles "loops" that run and the code to make it cleaner and faster.
  * UPDATE: Threading and caching greatly reduces overloading orleans
* There is a cost of CPU to running this mod. You need a decent setup or scale down the sectors generated.
  * UPDATE: CPU cost has been reduced with threading and caching

# Advantages of Running the mod on a separate container

* No need to restart the server, just the mod container
* The way the mod import is done is via assembly load and reflection. There are LOTS of conflicts with assemblies if you need anything extra like I do on this mode (see the code). Easier to have it as a separate container

# Features

* Sector Spawner
* Sector Spatial Hashing
* Factions
* Faction Territory
* Dynamic Scripts
* Element Extended Properties

# Quick Start

* Pull the docker container: [Docker Container](https://hub.docker.com/repository/docker/voidrunner7891/dynamic_encounters/general)
* `docker pull voidrunner7891/dynamic_encounters`
* Add it to the docker-compose of myDU:
```yaml
  mod_dynamic_encounters:
    image: voidrunner7891/dynamic_encounters
    ports:
      - "8080:8080"
    environment:
      QUEUEING: http://queueing:9630
      BOT_LOGIN: ${PVE_BOT_USERNAME}
      BOT_PASSWORD: ${PVE_BOT_PASSWORD}
      BOT_PREFIX: ${PVE_BOT_PREFIX}
      CORS_ALLOW_ALL: 'true'
      API_ENABLED: 'true'
    volumes:
      - ${DATAPATH}:/data
      - ${LOGPATH}:/logs
      - ${CONFPATH}:/config
    networks:
      vpcbr:
        ipv4_address: 10.5.0.21
```
* Modify your postgres image to use postgis - for geo-spatial querying
```yaml
  postgres:
    image: postgis/postgis:11-3.3
```

* Create a bot account on the backoffice - you need the roles Game and Bot at least
* Append to the `.env` file on mydu-server root folder with the credentials of the bot and a name for the Chat window - PVE for instance

```env
PVE_BOT_PREFIX=PVE
PVE_BOT_USERNAME=bot
PVE_BOT_PASSWORD=botpassword
```

* Append to the `up.bat` or `up.sh` script

for bat:
```bat
timeout /t 5 /nobreak
docker-compose up -d mod_dynamic_encounters
```

for sh:
```sh
sleep 5
docker-compose up -d mod_dynamic_encounters
```

* Run the up script or `docker-compose up -d mod_dynamic_encounters` directly
* For debugging, tail the logs with `tail -f logs/Mod.log` or similar.

# Running Locally

see [Running Locally](Documentation/RunningLocally.md)

# Setting up Encounters

[Quick Start via APIS](Documentation/QuickStartApis.md)

[Encounters Setup OLD](Documentation/EncountersSetup.md)

# Roadmap - Not quite in Order

* ~~Enhanced/Scriptable Wreck Experience~~
* ~~NPCs that Shoot Back and have scriptable behaviors~~
* Factions and Influence - Like Elite Dangerous BGS
* User Interface
* Second Pass on NPCs and Wreck Experience
