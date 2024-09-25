using System.Threading.Tasks;
using BotLib.Generated;
using Microsoft.AspNetCore.Mvc;
using Mod.DynamicEncounters.Common;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.RDMS;

namespace Mod.DynamicEncounters.Api.Controllers;

[Route("rdms")]
public class RDMSController : Controller
{
    [Route("give-rights-to-bot")]
    [HttpPost]
    public async Task<IActionResult> GiveRightsToBot()
    {
        var provider = ModBase.ServiceProvider;
        var orleans = provider.GetOrleans();
        var bank = provider.GetGameplayBank();
        
        var unknownEntity = new EntityId { playerId = StaticPlayerId.Unknown };

        var actor = await ModBase.Bot.Req.RDMSActorCreate(new ActorData
        {
            name = "Bot Rights",
            description = "Rights to Bot",
            owner = unknownEntity,
            actorId = new ActorId { type = ActorType.Player, actorId = ModBase.Bot.PlayerId }
        });

        await ModBase.Bot.Req.RDMSPolicyCreate(
            new PolicyData
            {
                tags = [new TagId{type = TagType.All}],
                actors = [actor],
                owner = unknownEntity,
                name = "Rights to Bot",
                rights =
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
                ]
            }
        );

        return Ok();
    }
}