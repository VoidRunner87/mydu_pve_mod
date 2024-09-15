# Running the Mod Code on Local PC

Expose these ports on your docker compose
Below is my docker compose for reference:

```yaml
version: "3.1"

networks:
  vpcbr:
    driver: bridge
    ipam:
      config:
        - subnet: 10.5.0.0/16

services:
  prometheus:
    image: prom/prometheus
    ports:
      - "127.0.0.1:9090:9090"
    volumes:
      - ./prometheus-config:/etc/prometheus
      - ./prometheus-data:/prometheus
    networks:
      vpcbr:
        ipv4_address: 10.5.0.101
  logrotate:
    image: ${NQREPO}dual-server-logrotate:$NQVERSION
    volumes:
      - ./config:/config
      - ./logs:/logs
    networks:
      vpcbr:
        ipv4_address: 10.5.0.201
  smtp:
    image: mwader/postfix-relay
    environment:
      POSTFIX_myhostname: dual.dev
    networks:
      vpcbr:
        ipv4_address: 10.5.0.200
  nginx:
    image: nginx
    volumes:
      - ./nginx:/etc/nginx
      - ./data:/data # need access to user_content
      - ./letsencrypt:/etc/letsencrypt
    ports:
      - "9630:9630" # queueing service
      # - "10111:10111" # orleans (construct elements)
      # - "8081:8081"   # voxels
      - "12000:12000" # backoffice
      - "10000:10000" # servers user_content
      - "443:443" # <-- enable this for SSL mode
    networks:
      vpcbr:
        ipv4_address: 10.5.0.100
    restart: always
  mongo:
    image: mongo:7.0
    environment:
      MONGO_INITDB_ROOT_USERNAME: mongo
      MONGO_INITDB_ROOT_PASSWORD: mongo
    volumes:
      - ${DBPATH}/db-mongo:/data/db
    networks:
      vpcbr:
        ipv4_address: 10.5.0.7
    ports:
      - "27017:27017"
  redis:
    image: redis:5.0
    networks:
      vpcbr:
        ipv4_address: 10.5.0.8
    ports:
      - "6379:6379"
  postgres:
    image: postgres:11.2
    environment:
      POSTGRES_PASSWORD: postgres
    volumes:
      - ${DBPATH}/db-postgres:/var/lib/postgresql/data
    networks:
      vpcbr:
        ipv4_address: 10.5.0.9
    ports:
      - "5432:5432"
  rabbitmq:
    image: rabbitmq:3.13.6
    networks:
      vpcbr:
        ipv4_address: 10.5.0.10
  zookeeper:
    image: confluentinc/cp-zookeeper:latest
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
      ZOOKEEPER_TICK_TIME: 2000
    networks:
      vpcbr:
        ipv4_address: 10.5.0.11
  kafka:
    image: confluentinc/cp-kafka:latest
    depends_on:
      - zookeeper
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:9092
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT
      KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
    networks:
      vpcbr:
        ipv4_address: 10.5.0.12
  front:
    image: ${NQREPO}dual-server-front:$NQVERSION
    volumes:
      - ${CONFPATH}:/config
      - ${LOGPATH}:/logs
      - ${DATAPATH}:/data
    command: /config/dual.yaml
    ports:
      - "9210:9210"
    restart: always
    networks:
      vpcbr:
        ipv4_address: 10.5.0.5
  node:
    image: ${NQREPO}dual-server-node:$NQVERSION
    command: /config/dual.yaml
    volumes:
      - ${CONFPATH}:/config
      - ${DATAPATH}:/data
      - ${LOGPATH}:/logs
    restart: always
    networks:
      vpcbr:
        ipv4_address: 10.5.0.6
  orleans:
    image: ${NQREPO}dual-server-orleans:$NQVERSION
    command: /config/dual.yaml
    volumes:
      - ${CONFPATH}:/config
      - ${DATAPATH}:/data
      - ${LOGPATH}:/logs
      - ./Mods:/OrleansGrains/Mods
    restart: always
    networks:
      vpcbr:
        ipv4_address: 10.5.0.13
    ports:
      - "30000:30000"
      - "10111:10111"
  constructs:
    image: ${NQREPO}dual-server-constructs:$NQVERSION
    command: /config/dual.yaml
    volumes:
      - ${CONFPATH}:/config
      - ${LOGPATH}:/logs
    restart: always
    networks:
      vpcbr:
        ipv4_address: 10.5.0.14
  queueing:
    image: ${NQREPO}dual-server-queueing:$NQVERSION
    command: /config/dual.yaml
    volumes:
      - ${CONFPATH}:/config
      - ${LOGPATH}:/logs
    networks:
      vpcbr:
        ipv4_address: 10.5.0.15
  voxel:
    image: ${NQREPO}dual-server-voxel:$NQVERSION
    command: /config/dual.yaml
    volumes:
      - ${CONFPATH}:/config
      - ${LOGPATH}:/logs
    restart: always
    networks:
      vpcbr:
        ipv4_address: 10.5.0.16
    ports:
      - "8081:8081"
  market:
    image: ${NQREPO}dual-server-market:$NQVERSION
    command: /config/dual.yaml
    volumes:
      - ${CONFPATH}:/config
      - ${DATAPATH}:/data
      - ${LOGPATH}:/logs
    restart: always
    networks:
      vpcbr:
        ipv4_address: 10.5.0.17
  backoffice:
    image: ${NQREPO}dual-server-backoffice:$NQVERSION
    command: /config/dual.yaml
    volumes:
      - ${CONFPATH}:/config
      - ${DATAPATH}:/data
      - ${LOGPATH}:/logs
    restart: always
    networks:
      vpcbr:
        ipv4_address: 10.5.0.18
  nodemanager:
    image: ${NQREPO}dual-server-nodemanager:$NQVERSION
    command: /config/dual.yaml
    volumes:
      - ${CONFPATH}:/config
      - ${LOGPATH}:/logs
    restart: always
    networks:
      vpcbr:
        ipv4_address: 10.5.0.19
  sandbox:
    image: ${NQREPO}dual-server-python:$NQVERSION
    command: "sleep 360000"
    volumes:
      - ${CONFPATH}:/config
      - ${DATAPATH}:/data
      - ${LOGPATH}:/logs
      - ./nginx:/etc/nginx
      - ./letsencrypt:/etc/letsencrypt
    networks:
      vpcbr:
        ipv4_address: 10.5.0.20
```

Adjust the `dual.yaml` file `s3.override_base_path` property relative to where the data folder is. Example:

```yaml
s3:
    override_base_path: ..\..\..\data
```


Then checkout the latest version of the code and run.
