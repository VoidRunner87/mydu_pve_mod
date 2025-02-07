using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Backend;
using Backend.Construct;
using Backend.Database;
using Microsoft.Extensions.Logging;
using NQ;
using NQutils;
using NQutils.Core.Shared;
using NQutils.Sql;

namespace Mod.DynamicEncounters.Stubs;

public class PlanetListStub : IPlanetList, IAsyncInitializable
  {
    private readonly ILogger<PlanetList> _logger;
    private readonly ISql _sql;
    private Dictionary<ConstructId, ConstructInfo> _planets = new();
    private readonly List<Sphere> _safeZones = new();
    private readonly List<Sphere> _planetarySafeZones = new();

    public PlanetListStub(
      ILogger<PlanetList> logger,
      ISql sql)
    {
      _logger = logger;
      _sql = sql;
    }

    public IEnumerable<ConstructInfo> GetPlanetList()
    {
      return _planets.Values;
    }

    public async Task<ConstructCharacteristics?> GetPlanet(ConstructId id, bool allowDeleted = false)
    {
      ConstructInfo info;
      return _planets.TryGetValue(id, out info) ? info.ToCharacteristics() : (await Fetch(id, allowDeleted)).ToCharacteristics();
    }

    public bool IsPlanet(ConstructId constructId)
    {
      return _planets.ContainsKey(constructId.constructId);
    }

    public async Task<ConstructGeometry?> GetPlanetGeometry(ConstructId id)
    {
      return (await GetPlanet(id))?.geometry;
    }

    public async Task<PlanetProperties?> GetPlanetProperties(ConstructId id)
    {
      return (await GetPlanet(id))?.planetProperties;
    }

    public async Task<ConstructInfo> Fetch(ConstructId id, bool allowDeleted = false)
    {
      return (await _sql.GetConstruct(id, allowDeleted)).ToConstructInfo();
    }

    public async Task<uint> GetTileIndex(ConstructId planetId, Vec3 pos, bool allowDeleted)
    {
      ConstructCharacteristics planet = await GetPlanet(planetId, allowDeleted);
      PlanetProperties planetProperties = planet != null ? planet.planetProperties : throw new Exception("No such planet");
      ConstructGeometry geometry = planet.geometry;
      Vec3 pos1 = new Vec3()
      {
        x = pos.x - geometry.size / 2L,
        y = pos.y - geometry.size / 2L,
        z = pos.z - geometry.size / 2L
      };
      int tileIndex = Tile.NQFindTileIndex(planetProperties.altitudeReferenceRadius, planetProperties.territoryTileSize, ref pos1);
      return tileIndex != -1 ? (uint) tileIndex : throw new Exception("Bad coordinates");
    }

    public async Task InitializeAsync(CancellationToken token)
    {
      _planets = (await _sql.GetPlanetList()).Where((Func<ConstructModel, bool>) (c => ConstructId.IsPlanet(c.Id.Value))).ToDictionary((Func<ConstructModel, ConstructId>) (c => c.Id.Value), (Func<ConstructModel, ConstructInfo>) (c => c.ToConstructInfo()));
    }

    public bool IsInSafeZone(Vec3 point)
    {
      return _safeZones.Any((Func<Sphere, bool>) (s => point.Dist2(s.center) < s.SqRadius));
    }

    public bool IsInPlanetarySafeZone(Vec3 point)
    {
      return _planetarySafeZones.Any((Func<Sphere, bool>) (s => point.Dist2(s.center) < s.SqRadius));
    }

    public List<Sphere> GetPlanetSafeZones() => _planetarySafeZones;
  }