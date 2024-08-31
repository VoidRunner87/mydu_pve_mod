# PVE Mod for MyDU

Before starting, know this:
* There were several hacky adjustments I had to make for this to work.
* I first focused on getting the solution ready to run as a separate docker container.
* After the basic functionality I envision for this mod is done, I'll start looking into optimizing the cycles "loops" that run and the code to make it cleaner and faster.
* There is a cost of CPU to running this mod. You need a decent setup or scale down the sectors generated.

# Quick Start

* Pull the docker container: [Docker Container](https://hub.docker.com/repository/docker/voidrunner7891/dynamic_encounters/general)
* `docker pull voidrunner7891/dynamic_encounters`
* Add it to the docker-compose of myDU:
```
  mod_dynamic_encounters:
    image: dynamic_encounters
    environment:
      BOT_LOGIN: ${PVE_BOT_USERNAME}
      BOT_PASSWORD: ${PVE_BOT_PASSWORD}
      BOT_PREFIX: ${PVE_BOT_PREFIX}
    volumes:
      - ${DATAPATH}:/data
      - ${LOGPATH}:/logs
    networks:
      vpcbr:
        ipv4_address: 10.5.0.21
```

* Create a bot account on the backoffice - you need the roles Game and Bot at least
* Append to the `.env` file on mydu-server root folder with the credentials of the bot and a name for the Chat window - PVE for instance

```
PVE_BOT_PREFIX=PVE
PVE_BOT_USERNAME=bot
PVE_BOT_PASSWORD=botpassword
```

* Append to the `up.bat` or `up.sh` script

for bat:
```
timeout /t 5 /nobreak
docker-compose up -d mod_dynamic_encounters
```

for sh:
```
sleep 5
docker-compose up -d mod_dynamic_encounters
```

* Run the up script or `docker-compose up -d mod_dynamic_encounters` directly
* For debugging, tail the logs with `tail -f logs/Mod.log` or similar.

# Roadmap - Not in Order

* Enhanced/Scriptable Wreck Experience
* NPCs that Shoot Back and have scriptable behaviors
* 
* Warp Gates
