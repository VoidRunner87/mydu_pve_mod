using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend;
using Backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQutils.Def;
using NQutils.Sql;
using Services;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("element")]
public class ElementController : Controller
{
    [HttpGet]
    [Route("fueltank")]
    public IActionResult GetFuelTanks()
    {
        var provider = ModBase.ServiceProvider;
        var bank = provider.GetGameplayBank();

        var fuelContainer = bank.GetDefinition<FuelContainer>();

        var items = new List<Dictionary<string, object>>();

        AddFuelTankItem(fuelContainer, items);

        return Ok(items);
    }

    [HttpGet]
    [Route("engine")]
    public IActionResult GetEngines()
    {
        var provider = ModBase.ServiceProvider;
        var bank = provider.GetGameplayBank();

        var atmoEngine = bank.GetDefinition<AtmosphericEngine>();
        var spaceEngine = bank.GetDefinition<SpaceEngine>();
        var rocketEngine = bank.GetDefinition<RocketEngine>();

        var items = new List<Dictionary<string, object>>();

        AddEngineItem(atmoEngine, items);
        AddEngineItem(spaceEngine, items);
        AddEngineItem(rocketEngine, items);

        return Ok(items);
    }

    private void AddFuelTankItem(IGameplayDefinition item, List<Dictionary<string, object>> map)
    {
        var children = item.GetChildren().ToList();

        if (children.Count == 0)
        {
            if (item.BaseObject is FuelContainer fuelContainer)
            {
                map.Add(new Dictionary<string, object>
                {
                    { "id", item.Id },
                    { "name", item.Name },
                    { "maxVolume", fuelContainer.maxVolume },
                    { "unitMass", fuelContainer.unitMass },
                    { "hitpoints", fuelContainer.hitpoints },
                    { "scale", fuelContainer.scale },
                });
            }

            return;
        }

        foreach (var child in children)
        {
            AddFuelTankItem(child, map);
        }
    }

    private void AddEngineItem(IGameplayDefinition item, List<Dictionary<string, object>> map)
    {
        var children = item.GetChildren().ToList();

        if (children.Count == 0)
        {
            if (item.BaseObject is EngineDisplay engine)
            {
                map.Add(new Dictionary<string, object>
                {
                    { "Name", item.Name },
                    { "FuelRate", engine.fuelRate },
                    { "MaxPower", engine.maxPower },
                });
            }

            return;
        }

        foreach (var child in children)
        {
            AddEngineItem(child, map);
        }
    }

    [HttpGet]
    [Route("databank/construct/{constructId:long}/element/{elementId:long}")]
    public async Task<IActionResult> GetDataBankData(ulong constructId, ulong elementId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var constructElementsGrain = orleans.GetConstructElementsGrain(constructId);
        var propertyValue = await constructElementsGrain.GetPropertyValue(new ElementPropertyId
        {
            elementId = elementId,
            constructId = constructId,
            name = "databank"
        });

        return Ok(propertyValue.value);
    }

    [HttpGet]
    [Route("{constructId:long}/bbox")]
    public async Task<IActionResult> CalculateBoundingBox(ulong constructId)
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();

        var sql = provider.GetRequiredService<ISql>();
        var elements = await sql.GetElementsInConstruct(constructId);

        var elementBoundingBox = provider.GetRequiredService<IElementBoundingBox>();
        var constructBoundingBox = provider.GetRequiredService<IConstructBoundingBox>();

        var list = new List<BoundingBox>();

        var constructInfoGrain = orleans.GetConstructInfoGrain(constructId);
        var info = await constructInfoGrain.Get();

        foreach (var elementInfo in elements)
        {
            var bbox = elementBoundingBox.GetBoundingBoxInConstruct(
                elementInfo.elementType,
                elementInfo.position,
                elementInfo.rotation,
                (int)info.rData.geometry.size
            );

            list.Add(bbox);
        }

        return Ok(new
        {
            V1 = CalculateEncompassingBoundingBox(list),
            V2 = constructBoundingBox.GetConstructBoundingBox(constructId)
        });
    }

    public static BoundingBox CalculateEncompassingBoundingBox(List<BoundingBox> boxes)
    {
        if (boxes == null || boxes.Count == 0)
        {
            throw new ArgumentException("The list of bounding boxes cannot be null or empty.");
        }

        // Start with extreme values to compare against
        Vec3 globalMin = new Vec3
        {
            x = double.MaxValue,
            y = double.MaxValue,
            z = double.MaxValue
        };

        Vec3 globalMax = new Vec3
        {
            x = double.MinValue,
            y = double.MinValue,
            z = double.MinValue
        };

        // Iterate through the list of bounding boxes
        foreach (var box in boxes)
        {
            // Update global minimums
            globalMin.x = Math.Min(globalMin.x, box.min.x);
            globalMin.y = Math.Min(globalMin.y, box.min.y);
            globalMin.z = Math.Min(globalMin.z, box.min.z);

            // Update global maximums
            globalMax.x = Math.Max(globalMax.x, box.max.x);
            globalMax.y = Math.Max(globalMax.y, box.max.y);
            globalMax.z = Math.Max(globalMax.z, box.max.z);
        }

        // Create and return the encompassing bounding box
        return new BoundingBox
        {
            min = globalMin,
            max = globalMax
        };
    }
}