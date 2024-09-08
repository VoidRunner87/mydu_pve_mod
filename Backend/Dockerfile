FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src
COPY ["Mod.DynamicEncounters.csproj", "./"]
RUN dotnet restore "Mod.DynamicEncounters.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "Mod.DynamicEncounters.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Mod.DynamicEncounters.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

COPY /Features/Spawner/Scripts /app/Scripts

RUN apk add --no-cache \
      gcc \
      g++ \
      make \
      libc-dev \
      libstdc++ 

RUN apk add gcompat
RUN apk add musl-dev

EXPOSE 9210

CMD ["sh", "-c", "dotnet Mod.DynamicEncounters.dll ../config/dual.yaml"]