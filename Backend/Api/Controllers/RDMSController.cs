using System.Linq;
using System.Threading.Tasks;
using BotLib.Generated;
using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Common.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;
using NQ.RDMS;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("rdms")]
public class RdmsController : Controller
{
    [Route("give-rights")]
    [HttpPost]
    public async Task<IActionResult> GiveRights([FromBody] GiveRightsRequest request)
    {
        var orleans = ModBase.ServiceProvider.GetOrleans();

        var keyName = $"Full Rights to {request.ToPlayerId}";

        var entity = new EntityId { playerId = request.FromPlayerId };

        var rdmsRightGrain = orleans.GetRDMSRegistryGrain(entity);
        var actorDataList = await rdmsRightGrain.GetActorDataList();
        var actorData = actorDataList.actors
            .FirstOrDefault(x => x.name == keyName);

        if (actorData != null)
        {
            await ModBase.Bot.Req.RDMSActorDelete(
                new ActorSelector
                {
                    owner = entity,
                    actorId = actorData.actorId
                }
            );
        }

        actorData = new ActorData
        {
            actorId = new ActorId { type = ActorType.Player, actorId = request.ToPlayerId },
            owner = entity,
            name = keyName,
            description = $"Full Rights to {request.ToPlayerId}"
        };

        await ModBase.Bot.Req.RDMSActorCreate(actorData);

        var policyDataList = await rdmsRightGrain.GetPolicyDataList();
        var fullRightPolicy = policyDataList.policies.FirstOrDefault(x => x.name == actorData.name);

        Right[] rights =
        [
            Right.ConstructBuild,
            Right.ConstructManeuver,
            Right.ConstructRename,
            Right.ConstructBlueprint,
            Right.ConstructTokenize,
            Right.ConstructAbandon,
            Right.ConstructParent,
            Right.ConstructBoard,
            Right.ConstructSnapshot,
            Right.ConstructRepair,
            Right.ConstructUseJetpack,
            Right.ConstructCreate,
            Right.ElementUse,
            Right.ElementEdit,
            Right.ElementRename,
            Right.TerritoryDig,
            Right.TerritoryMine,
            Right.TerritoryHarvest,
            Right.TerritoryRemove,
            Right.TerritoryMiningUnit,
            Right.ItemSell,
            Right.ItemBarter,
            Right.ItemDeploy,
            Right.ItemDestroy,
            Right.WalletConsult,
            Right.WalletAdd,
            Right.WalletTake,
            Right.OrganizationRecruit,
            Right.OrganizationFire,
            Right.OrganizationViewTerritories,
            Right.DeployOnConstruct,
            Right.DeployOverlapConstruct,
            Right.DeployStaticOnTerritory,
            Right.DeployDynamicOnTerritory,
            Right.ContainerView,
            Right.ContainerPut,
            Right.ContainerRetrieve,
            Right.AssetTag,
            Right.SPSConnect,
            Right.IndustryEditRecipeBank,
            Right.WalletEdit,
            Right.Count
        ];

        if (fullRightPolicy == null)
        {
            await ModBase.Bot.Req.RDMSPolicyCreate(
                new PolicyData
                {
                    tags = [new TagId { type = TagType.All }],
                    actors = [actorData.actorId],
                    owner = entity,
                    name = actorData.name,
                    rights = rights.ToList()
                }
            );
        }
        else
        {
            await ModBase.Bot.Req.RDMSPolicyUpdate(
                new PolicyData
                {
                    policyId = fullRightPolicy.policyId,
                    tags = [new TagId { type = TagType.All }],
                    actors = [actorData.actorId],
                    owner = entity,
                    name = actorData.name,
                    rights = rights.ToList()
                }
            );
        }

        return Ok();
    }

    public class GiveRightsRequest
    {
        public ulong FromPlayerId { get; set; } = StaticPlayerId.Unknown;
        public string ActorName { get; set; } = "Full Rights";
        public ulong ToPlayerId { get; set; } = ModBase.Bot.PlayerId;
    }
}