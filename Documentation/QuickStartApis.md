# Quick Start via APIs

* Create a bot account with the roles `Game` and `bot`

## Docker Compose - if running on docker

Run the mod with extra environment variables:

```yaml
  mod_dynamic_encounters:
    image: dynamic_encounters
    ports:
      - "8080:8080"
    environment:
      BOT_LOGIN: ${PVE_BOT_USERNAME} # your bot username
      BOT_PASSWORD: ${PVE_BOT_PASSWORD} # your bot password
      BOT_PREFIX: ${PVE_BOT_PREFIX} # your bot ingame name
      API_ENABLED: 'true' # Enable APIs
      CORS_ALLOW_ALL: 'true' # Enables CORS. Consider security by allowing requests to come from any origin with this enabled. Adjust the domains of the container for optimal settings and disable cors.
    volumes:
      - ${DATAPATH}:/data
      - ${LOGPATH}:/logs
      - ${CONFPATH}:/config
    networks:
      vpcbr:
        ipv4_address: 10.5.0.21
```

The port will be `8080`

## Locally from the Code

If you're running locally, make sure to set the environment variables:

* API_ENABLED=true
* CORS_ALLOW_ALL=true

The port will be `5000`

## Access the Swagger

### Locally

[http://localhost:5000/swagger/](http://localhost:5000/swagger/)

### Running on Docker

[http://mod_dynamic_encounters:8080/swagger](http://mod_dynamic_encounters:8080/swagger) or [http://localhost:8080/swagger](http://localhost:8080/swagger)

### Setting up Data

You can also hit those endpoints using a tool like postman.

#### 1. Use The Starter Content API

Hit the "StarterContent" Api Single POST Endpoint.

This will install a basic POI and Basic Pirate NPC and also will enable all necessary features.

#### 2. Use the Wreck and NPC endpoints

Use the wreck and NPC endpoints to add a wreck or an aggressive NPC.

The NPC api needs a POI (wreck) construct. You can use the basic poi that comes with the starter content. Tip: Use the `Get /script` to see a list of scripts you have available.

#### 3. Use the Sector Encounter Add Wreck and Add NPC endpoints

These endpoints will add encounter options to the pool when a sector is generated

#### 4. Restart the Service

Currently Scripts are cached in memory. So it needs a restart.

#### 5. Force Expire all Sectors

Use the Sector Instance Force Expire endpoints `/sector/instance/expire/force/all` to instantly regenerate all sectors.
