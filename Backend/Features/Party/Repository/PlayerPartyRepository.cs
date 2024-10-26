using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Party.Data;
using Mod.DynamicEncounters.Features.Party.Interfaces;
using Newtonsoft.Json;
using NQ;

namespace Mod.DynamicEncounters.Features.Party.Repository;

public class PlayerPartyRepository(IServiceProvider provider) : IPlayerPartyRepository
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task<bool> IsInAParty(PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        return await db.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(0) FROM public.mod_player_party WHERE player_id = @id
            """,
            new { id = (long)playerId.id }
        ) > 0;
    }

    public async Task<bool> IsAcceptedMember(PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        return await db.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(0) FROM public.mod_player_party 
            WHERE player_id = @id AND
                  is_pending_accept_request IS FALSE AND
                  is_pending_accept_invite IS FALSE
            """,
            new { id = (long)playerId.id }
        ) > 0;
    }

    public async Task<PlayerPartyGroupId> CreateParty(PlayerId leaderPlayerId)
    {
        using var db = _factory.Create();
        db.Open();

        var groupId = Guid.NewGuid();

        await db.ExecuteAsync(
            """
            INSERT INTO public.mod_player_party (group_id, player_id, is_leader, is_pending_accept_request, is_pending_accept_invite, json_properties) 
            VALUES (
                @group_id,
                @player_id,
                true,
                false,
                false,
                @json_properties::jsonb
            )
            """,
            new
            {
                group_id = groupId,
                player_id = (long)leaderPlayerId.id,
                json_properties = JsonConvert.SerializeObject(
                    new PlayerPartyItem.PartyProperties()
                )
            }
        );

        return new PlayerPartyGroupId(groupId);
    }

    public async Task AddPendingPartyRequest(PlayerPartyGroupId groupId, PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO public.mod_player_party (group_id, player_id, is_leader, is_pending_accept_request, is_pending_accept_invite, json_properties) 
            VALUES (
                @group_id,
                @player_id,
                false,
                true,
                false,
                @json_properties::jsonb
            )
            """,
            new
            {
                group_id = groupId.Id,
                player_id = (long)playerId.id,
                json_properties = JsonConvert.SerializeObject(
                    new PlayerPartyItem.PartyProperties()
                )
            }
        );
    }

    public async Task AddPendingPartyInvite(PlayerPartyGroupId groupId, PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO public.mod_player_party (group_id, player_id, is_leader, is_pending_accept_request, is_pending_accept_invite, json_properties) 
            VALUES (
                @group_id,
                @player_id,
                false,
                false,
                true,
                @json_properties::jsonb
            )
            """,
            new
            {
                group_id = groupId.Id,
                player_id = (long)playerId.id,
                json_properties = JsonConvert.SerializeObject(
                    new PlayerPartyItem.PartyProperties()
                )
            }
        );
    }

    public async Task AcceptPendingInvite(PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            UPDATE public.mod_player_party SET is_pending_accept_invite = FALSE WHERE player_id = @player_id
            """,
            new
            {
                player_id = (long)playerId.id,
            }
        );
    }

    public async Task AcceptPartyRequest(PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            UPDATE public.mod_player_party SET is_pending_accept_request = FALSE WHERE player_id = @player_id
            """,
            new
            {
                player_id = (long)playerId.id,
            }
        );
    }

    public async Task SetPlayerPartyRole(PlayerId playerId, string role)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            UPDATE public.mod_player_party 
            SET json_properties = jsonb_set(json_properties, '{role}', @role::jsonb)
            WHERE player_id = @player_id
            """,
            new
            {
                player_id = (long)playerId.id,
                role = $"\"{role}\""
            }
        );
    }

    public async Task<PlayerPartyGroupId> FindPartyGroupId(PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        var groupIdGuid = await db.ExecuteScalarAsync<Guid>(
            """
            SELECT group_id FROM public.mod_player_party WHERE player_id = @id
            """,
            new { id = (long)playerId.id }
        );

        return new PlayerPartyGroupId(groupIdGuid);
    }

    public async Task<bool> IsPartyLeader(PlayerPartyGroupId groupId, PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        return await db.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(0) FROM public.mod_player_party 
            WHERE group_id = @group_id AND player_id = @id AND is_leader = true
            """,
            new
            {
                id = (long)playerId.id,
                group_id = groupId.Id
            }
        ) > 0;
    }

    public async Task DisbandParty(PlayerPartyGroupId groupId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync("DELETE FROM public.mod_player_party WHERE group_id = @id", new { id = groupId.Id });
    }

    public async Task RemoveNonLeaderPlayerFromParty(PlayerPartyGroupId groupId, PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            "DELETE FROM public.mod_player_party WHERE group_id = @id AND player_id = @player_id AND is_leader IS FALSE",
            new { id = groupId.Id, playerId = (long)playerId.id }
        );
    }

    public async Task RemovePlayerFromPartyAndFindNewLeader(PlayerPartyGroupId groupId, PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        var transaction = db.BeginTransaction();

        await db.ExecuteAsync(
            "DELETE FROM public.mod_player_party WHERE group_id = @id AND player_id = @player_id",
            new
            {
                id = groupId.Id,
                player_id = (long)playerId.id
            },
            transaction
        );

        var leaderCount = await db.ExecuteScalarAsync<int>(
            """
            SELECT COUNT(0) FROM public.mod_player_party 
            WHERE group_id = @group_id AND is_leader = true
            """,
            new
            {
                group_id = groupId.Id
            },
            transaction
        );

        if (leaderCount == 0)
        {
            var nextLeader = await db.ExecuteScalarAsync<long?>(
                """
                SELECT player_id FROM public.mod_player_party 
                WHERE group_id = @id AND 
                      is_pending_accept_invite IS FALSE AND  
                      is_pending_accept_request IS FALSE
                ORDER BY created_at LIMIT 1
                """,
                new { id = groupId.Id }
            );

            if (nextLeader != null)
            {
                await db.ExecuteAsync(
                    """
                    UPDATE public.mod_player_party 
                    SET is_leader = true 
                    WHERE player_id = @player_id
                    """,
                    new
                    {
                        player_id = nextLeader
                    },
                    transaction
                );
            }
            else
            {
                await db.ExecuteAsync(
                    "DELETE FROM public.mod_player_party WHERE group_id = @group_id",
                    new { group_id = groupId.Id },
                    transaction
                );
            }
        }

        transaction.Commit();
    }

    public async Task SetPartyLeader(PlayerPartyGroupId groupId, PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            UPDATE public.mod_player_party SET is_leader = CASE WHEN player_id = @player_id THEN TRUE ELSE FALSE END
            WHERE group_id = @group_id
            """,
            new
            {
                player_id = (long)playerId.id,
                group_id = groupId.Id
            }
        );
    }

    public async Task<IEnumerable<PlayerPartyItem>> GetPartyByPlayerId(PlayerId playerId)
    {
        using var db = _factory.Create();
        db.Open();

        var results = (await db.QueryAsync<DbRow>(
            """
            SELECT PP.*, P.display_name player_name, P.connected player_connected 
            FROM public.mod_player_party PP
            INNER JOIN public.player P ON P.id = PP.player_id
            WHERE group_id = (
                SELECT group_id FROM public.mod_player_party WHERE player_id = @player_id
            )
            """,
            new
            {
                player_id = (long)playerId.id
            }
        )).ToList();

        return results.Select(MapToModel);
    }

    private static PlayerPartyItem MapToModel(DbRow row)
    {
        return new PlayerPartyItem
        {
            Id = row.id,
            PlayerId = (ulong)row.player_id,
            PlayerName = row.player_name,
            IsConnected = row.player_connected,
            GroupId = row.group_id,
            CreatedAt = row.created_at,
            IsLeader = row.is_leader,
            IsPendingAcceptInvite = row.is_pending_accept_invite,
            IsPendingAcceptRequest = row.is_pending_accept_request,
            Properties = JsonConvert.DeserializeObject<PlayerPartyItem.PartyProperties>(row.json_properties)
        };
    }

    private struct DbRow
    {
        public Guid id { get; set; }
        public string player_name { get; set; }
        public bool player_connected { get; set; }
        public Guid group_id { get; set; }
        public long player_id { get; set; }
        public bool is_leader { get; set; }
        public bool is_pending_accept_request { get; set; }
        public bool is_pending_accept_invite { get; set; }
        public string json_properties { get; set; }
        public DateTime created_at { get; set; }
    }
}