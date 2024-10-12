using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Data;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;
using Mod.DynamicEncounters.Features.Scripts.Actions.Services;
using Mod.DynamicEncounters.Helpers;

namespace Mod.DynamicEncounters.Features.Scripts.Actions;

[ScriptActionName(ActionName, Description = Description)]
public class GiveElementSkinToPlayer(ScriptActionItem actionItem) : IScriptAction
{
    private readonly ScriptActionItem _actionItem = actionItem;
    public const string ActionName = "give-element-skin";
    public const string Description = "Gives an element skin to a player in the context of the execution";
    public string Name => ActionName;

    public string GetKey() => Name;

    public async Task<ScriptActionResult> ExecuteAsync(ScriptContext context)
    {
        var provider = context.ServiceProvider;
        var logger = provider.CreateLogger<GiveElementSkinToPlayer>();

        var playerService = provider.GetRequiredService<IPlayerService>();

        context.TryGetPropertyParsedAs("Skins", out var skinItems, new List<ElementSkinItem>());

        if (skinItems.Count == 0)
        {
            return ScriptActionResult.Failed();
        }

        foreach (var playerId in context.PlayerIds)
        {
            var map = await playerService.GetAllElementSkins(playerId);

            foreach (var skinItem in skinItems)
            {
                if (map.TryGetValue(skinItem.ElementTypeId, out var value) && value.Contains(skinItem.Skin))
                {
                    continue;
                }

                try
                {
                    await playerService.GivePlayerElementSkins(
                        playerId,
                        [
                            new IPlayerService.ElementSkinItem
                            {
                                ElementTypeId = skinItem.ElementTypeId,
                                Skin = skinItem.Skin
                            }
                        ]
                    );
                }
                catch (Exception e)
                {
                    logger.LogError(
                        e, 
                        "Failed to add Skin {El}-{Skin} to Player {Player}", skinItem.ElementTypeId,
                        skinItem.Skin, 
                        playerId
                    );
                }
            }
        }

        return ScriptActionResult.Successful();
    }

    public class ElementSkinItem
    {
        public ulong ElementTypeId { get; set; }
        public string Skin { get; set; }

        public bool IsValid() => !string.IsNullOrEmpty(Skin);
    }
}