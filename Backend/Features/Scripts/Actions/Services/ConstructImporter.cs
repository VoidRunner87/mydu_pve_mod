using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.Database;
using Backend.Fixture.Construct;
using NQ;
using NQutils.Core;
using NQutils.Def;
using NQutils.Sql;
using Serilog;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Services;

public static class ConstructImporter
{
    public static async Task<List<ulong>> Import(
        ConstructFixture fixture,
        IUserContent userContent,
        ISql sql,
        IVoxelImporter voxelService,
        IGameplayBank bank,
        IRDMSStorage rdms,
        IPlanetList planetList,
        ConstructSqlExtension.InsertMode mode = ConstructSqlExtension.InsertMode.Default,
        ulong setSandboxId = 0,
        ulong? setWormholeId = null)
    {
        // var tagMaps = await ConstructFixtureImport.WriteRDMS(fixture, sql, rdms);
        await WriteBlueprints(fixture, sql, bank, voxelService, rdms, userContent);
        var constructData1 = fixture.ToConstructData();
        if (setSandboxId != 0UL)
        {
            foreach (var constructData2 in constructData1)
                constructData2.Model.SandboxId = setSandboxId;
        }
        var ids = await Create(constructData1, userContent, sql, voxelService, rdms, planetList, mode, setWormholeId: setWormholeId);
        return ids;
    }
    
    private static async Task WriteBlueprints(
        ConstructFixture fixture,
        ISql sql,
        IGameplayBank bank,
        IVoxelImporter voxelService,
        IRDMSStorage rdms,
        IUserContent userContent)
    {
        if (!fixture.blueprints.Any())
        {
            return;
        }

        var remap = new List<(long, long)>();
        foreach (var blueprint in fixture.blueprints)
        {
            var bp = blueprint;
            var nullable = await sql.FindBlueprint(bp.Model.Name, bp.Model.CreatedAt);
            if (!nullable.HasValue || nullable.Value != (long) bp.Model.Id)
            {
                if (!nullable.HasValue)
                    nullable = new long?((long) await CreateBlueprint(bp, sql, voxelService, rdms, userContent));
                remap.Add(((long) bp.Model.Id, nullable.Value));
            }
        }
        foreach (var container in fixture.containers)
        {
            foreach (var storageSlot in container.content)
            {
                foreach (var valueTuple in remap)
                {
                    if (bank.GetDefinition(storageSlot.content.type)?.BaseObject is BlueprintBase && (long) storageSlot.content.id == valueTuple.Item1)
                    {
                        storageSlot.content.id = (ulong) valueTuple.Item2;
                        break;
                    }
                }
            }
        }

        foreach (var child in fixture.children)
        {
            await WriteBlueprints(child, sql, bank, voxelService, rdms, userContent);
        }
    }
    
    private static async Task<List<ulong>> Create(
      ConstructData[] cdata,
      IUserContent userContent,
      ISql sql,
      IVoxelImporter voxelService,
      IRDMSStorage rdms,
      IPlanetList planetList,
      ConstructSqlExtension.InsertMode mode = ConstructSqlExtension.InsertMode.Default,
      ulong? setWormholeId = null)
    {
      var results = new ConcurrentBag<ulong>();
      var idMap = new ConcurrentDictionary<ulong, ulong>();
      var taskMap = new Dictionary<ulong, List<Func<ulong, Task>>>();
      var ulongList2 = new List<ulong>();
      ulongList2.Add(cdata[0].Model.Id.GetValueOrDefault());
      for (var index = 1; index < cdata.Length; ++index)
      {
        var data = cdata[index];
        var func = (Func<ulong, Task>) (sid => CreateOne(data, userContent, sql, voxelService, rdms, planetList, mode, idMap, taskMap, results, sid, setWormholeId));
        var nullable = data.Model.ParentId;
        var num1 = (ulong) ((long?) nullable ?? (long) ulongList2[0]);
        if (!ulongList2.Contains(num1))
        {
          var num2 = ulongList2[0];
        }
        var dictionary1 = taskMap;
        nullable = data.Model.ParentId;
        var key1 = (long) nullable.Value;
        if (dictionary1.TryGetValue((ulong) key1, out var funcList2))
        {
          funcList2.Add(func);
        }
        else
        {
          var dictionary2 = taskMap;
          nullable = data.Model.ParentId;
          var key2 = (long) nullable.Value;
          dictionary2.Add((ulong) key2, new List<Func<ulong, Task>>()
          {
            func
          });
        }
        nullable = data.Model.Id;
        if (nullable.HasValue)
        {
          var ulongList3 = ulongList2;
          nullable = data.Model.Id;
          var num3 = (long) nullable.Value;
          ulongList3.Add((ulong) num3);
        }
      }
      var one = await CreateOne(cdata[0], userContent, sql, voxelService, rdms, planetList, mode, idMap, taskMap, results, 0UL, setWormholeId);
      var list = results.ToList<ulong>();
      list.Remove(one);
      var ulongList4 = new List<ulong>();
      ulongList4.Add(one);
      ulongList4.AddRange((IEnumerable<ulong>) list);
      return ulongList4;
    }
    
    private static void SanitizeProperties(ElementInfo element)
    {
      string val;
      if (!element.properties.TryGetProperty(NQutils.Def.Element.d_gameplayTag, out val) || !(val == ""))
        return;
      element.properties.Remove(NQutils.Def.Element.d_gameplayTag.name);
    }
    
    private static async Task<ulong> CreateOne(
      ConstructData data,
      IUserContent userContent,
      ISql sql,
      IVoxelImporter voxelService,
      IRDMSStorage rdms,
      IPlanetList planetList,
      ConstructSqlExtension.InsertMode mode,
      ConcurrentDictionary<ulong, ulong> idMap,
      Dictionary<ulong, List<Func<ulong, Task>>> taskMap,
      ConcurrentBag<ulong> results,
      ulong sandboxId,
      ulong? setWormholeId = null)
    {
      foreach (ElementInfo element in data.Elements)
      {
        await userContent.StoreProperties(element);
        SanitizeProperties(element);
      }
      ulong num1;
      if (data.Model.ParentId.HasValue && idMap.TryGetValue(data.Model.ParentId.Value, out num1))
        data.Model.ParentId = new ulong?(num1);
      if (sandboxId != 0UL && mode == ConstructSqlExtension.InsertMode.Sandbox)
        data.Model.SandboxId = new ulong?(sandboxId);
      ulong fixtureId = data.Model.Id.GetValueOrDefault();
      
      return await sql.AddConstruct(data, rdms, mode, (Func<ConstructId, Task>) (async constructId =>
      {
        results.Add(constructId);
        if (sandboxId == 0UL && mode == ConstructSqlExtension.InsertMode.Sandbox)
          sandboxId = constructId;
        if (fixtureId != 0UL)
          idMap.TryAdd(fixtureId, constructId);
        List<Func<ulong, Task>> todo = [];
        ulong? nullable;
        if (data.Model.Id.HasValue)
        {
          var dictionary = taskMap;
          nullable = data.Model.Id;
          var key = (long) nullable.Value;
          dictionary.TryGetValue((ulong) key, out _);
        }
        nullable = data.Model.ParentId;
        if (nullable.HasValue)
        {
          nullable = data.Model.ParentId;
          if (!ConstructId.IsPlanet(nullable.Value))
          {
            nullable = data.Model.ParentId;
            if (!ConstructId.IsSandbox(nullable.Value))
              goto label_15;
          }
          RelativeLocation loc = new RelativeLocation();
          loc.position = data.Model.Position;
          loc.rotation = data.Model.Rotation;
          nullable = data.Model.Size;
          Vec3 vec3 = loc.Center(nullable.GetValueOrDefault());
          try
          {
            IPlanetList planetList1 = planetList;
            nullable = data.Model.ParentId;
            if (nullable != null)
            {
              ConstructId planet = nullable.Value;
              Vec3 pos = vec3;
              uint tile = await planetList1.GetTileIndex(planet, pos, true);
              todo.Add(cid => sql.SetConstructTile(constructId, new ulong?(tile)));
            }
          }
          catch (Exception ex)
          {
            Log.Error(ex, "Failed to CreateOne");
          }
        }
        label_15:
        bool clear = mode == ConstructSqlExtension.InsertMode.Upsert || mode == ConstructSqlExtension.InsertMode.Sandbox;
        todo.Add(_ => voxelService.ImportFixture(new VoxelObject[1]
        {
          VoxelObject.Construct((ulong) constructId)
        }, data.VoxelData, (clear ? 1 : 0) != 0));
        await NQParallel.ForEachCap(todo, (Func<Func<ulong, Task>, Task>) (x => x(sandboxId)), 5);
        todo = null;
      }), setWormholeId);
    }
    
    private static async Task<ulong> CreateBlueprint(
        BlueprintData blueprint,
        ISql sql,
        IVoxelImporter voxelService,
        IRDMSStorage rdms,
        IUserContent userContent)
    {
        foreach (var element in blueprint.Elements)
        {
            await userContent.StoreProperties(element);
        }
        
        var id = await sql.AddBlueprint(blueprint, rdms);
        await voxelService.ImportFixture(new[]
        {
            VoxelObject.Blueprint((ulong) id)
        }, new VoxelData()
        {
            pipeline = null,
            cells = blueprint.VoxelData
        }, false);
        return id;
    }
}