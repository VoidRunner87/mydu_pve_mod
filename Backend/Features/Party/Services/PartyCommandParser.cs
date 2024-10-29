using System.Collections.Generic;
using System.Text;
using BotLib.Generated;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Party.Data;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.Party.Services;

public class PartyCommandParser : IPartyCommandParser
{
    public CommandHandlerOutcome Parse(ulong instigatorPlayerId, string command)
    {
        var pieces = new Queue<string>();
        var sb = new StringBuilder();

        foreach (var @char in command.Trim())
        {
            switch (@char)
            {
                case ' ':
                    if (sb.Length > 0)
                    {
                        pieces.Enqueue(sb.ToString());
                        sb.Clear();
                    }

                    break;
                case '@':
                    break;
                default:
                    sb.Append(@char);
                    break;
            }
        }

        if (sb.Length > 0)
        {
            pieces.Enqueue(sb.ToString());
        }

        pieces.Dequeue(); //@g
        var subCommand = pieces.Dequeue();

        switch (subCommand)
        {
            case "help":
                return CommandHandlerOutcome.Execute(async _ =>
                {
                    var messages = new List<string>
                    {
                        "> @g open - Opens the Group UI",
                        "> @g join PlayerName - Requests to join a player's group;",
                        "> @g invite PlayerName - Invites a player to the group;",
                        "> @g kick PlayerName - Kicks a player from the group;",
                        "> @g accept PlayerName - Accepts a player request to join the group;",
                        "> @g reject PlayerName - Rejects a player request to join the group;",
                        "> @g leave - Leaves the group;",
                        "> @g disband - Disbands the group;",
                        "> @g role commander - Sets your role as commander;"
                    };

                    foreach (var m in messages)
                    {
                        await ModBase.Bot.Req.ChatMessageSend(
                            new MessageContent
                            {
                                message = m,
                                channel = new MessageChannel
                                {
                                    channel = MessageChannelType.PRIVATE,
                                    targetId = instigatorPlayerId
                                }
                            }
                        );
                    }

                    return PartyOperationOutcome.Successful("Check DM for Help");
                });
            case "open":
                return CommandHandlerOutcome.Execute(
                    async _ =>
                    {
                        await ModBase.ServiceProvider.GetOrleans().GetModManagerGrain()
                            .TriggerModAction(instigatorPlayerId,
                                new ModAction { actionId = 103, modName = "Mod.DynamicEncounters" });

                        return PartyOperationOutcome.Successful("Loading Group UI");
                    });
            case "accept":
                if (pieces.Count == 0)
                {
                    return CommandHandlerOutcome.Failed("Missing player to accept. Ie: @accept PlayerName");
                }

                return CommandHandlerOutcome.Execute(service =>
                    service.AcceptPartyRequest(instigatorPlayerId, pieces.Dequeue())
                );
            case "reject":
                if (pieces.Count == 0)
                {
                    return CommandHandlerOutcome.Failed("Missing player to reject. Ie: @reject PlayerName");
                }

                return CommandHandlerOutcome.Execute(service =>
                    service.CancelPartyInviteRequest(instigatorPlayerId, pieces.Dequeue())
                );
            case "join":
                if (pieces.Count == 0)
                {
                    return CommandHandlerOutcome.Failed("Missing player to join. Ie: @join PlayerName");
                }

                return CommandHandlerOutcome.Execute(service =>
                    service.RequestJoinParty(instigatorPlayerId, pieces.Dequeue()));
            case "invite":
                if (pieces.Count == 0)
                {
                    return CommandHandlerOutcome.Failed("Missing player to invite. Ie: @invite PlayerName");
                }

                return CommandHandlerOutcome.Execute(service =>
                    service.InviteToParty(instigatorPlayerId, pieces.Dequeue()));
            case "kick":
                if (pieces.Count == 0)
                {
                    return CommandHandlerOutcome.Failed("Missing player to kick. Ie: @kick PlayerName");
                }

                return CommandHandlerOutcome.Execute(service =>
                    service.KickPartyMember(instigatorPlayerId, pieces.Dequeue()));
            case "promote":
                if (pieces.Count == 0)
                {
                    return CommandHandlerOutcome.Failed("Missing player to promote. Ie: @promote PlayerName");
                }

                return CommandHandlerOutcome.Execute(service =>
                    service.PromoteToPartyLeader(instigatorPlayerId, pieces.Dequeue()));
            case "leave":
                return CommandHandlerOutcome.Execute(service => service.LeaveParty(instigatorPlayerId));
            case "disband":
                return CommandHandlerOutcome.Execute(service => service.DisbandParty(instigatorPlayerId));
            case "role":
                if (pieces.Count == 0)
                {
                    return CommandHandlerOutcome.Failed("Missing role. Ie: @role commander");
                }
                
                return CommandHandlerOutcome.Execute(service => 
                    service.SetPlayerPartyRole(instigatorPlayerId, pieces.Dequeue()));
        }

        return CommandHandlerOutcome.Failed("Invalid Command");
    }
}