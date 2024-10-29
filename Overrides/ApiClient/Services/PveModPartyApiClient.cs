using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Overrides.ApiClient.Data;
using Mod.DynamicEncounters.Overrides.ApiClient.Interfaces;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Overrides.ApiClient.Services;

public class PveModPartyApiClient(IServiceProvider provider) : IPveModPartyApiClient
{
    private readonly IHttpClientFactory _httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();

    public async Task<IEnumerable<PlayerPartyItem>> GetPartyByPlayerId(ulong playerId, CancellationToken cancellationToken)
    {
        var url = Path.Combine(PveModBaseUrl.GetBaseUrl(), "party", $"{playerId}");
        
        using var client = _httpClientFactory.CreateClient();

        var responseMessage = await client.GetAsync(
            url,
            cancellationToken
        );

        var jsonString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        return JsonConvert.DeserializeObject<IEnumerable<PlayerPartyItem>>(jsonString);
    }

    public async Task<BasicOutcome> CancelInvite(ulong instigatorPlayerId, ulong playerId, CancellationToken cancellationToken)
    {
        return await PostAsync("party/invite/cancel", new
        {
            InstigatorPlayerId = instigatorPlayerId,
            PlayerId = playerId
        }, cancellationToken);
    }
    
    public async Task<BasicOutcome> CreateParty(ulong instigatorPlayerId, CancellationToken cancellationToken)
    {
        return await PostAsync("party/create", new
        {
            InstigatorPlayerId = instigatorPlayerId
        }, cancellationToken);
    }

    public async Task<BasicOutcome> LeaveParty(ulong instigatorPlayerId, CancellationToken cancellationToken)
    {
        return await PostAsync("party/leave", new
        {
            InstigatorPlayerId = instigatorPlayerId
        }, cancellationToken);
    }
    
    public async Task<BasicOutcome> DisbandParty(ulong instigatorPlayerId, CancellationToken cancellationToken)
    {
        return await PostAsync("party/disband", new
        {
            InstigatorPlayerId = instigatorPlayerId
        }, cancellationToken);
    }
    
    public async Task<BasicOutcome> SendPartyInvite(ulong instigatorPlayerId, ulong playerId, string playerName, CancellationToken cancellationToken)
    {
        return await PostAsync("party/invite/accept", new
        {
            InstigatorPlayerId = instigatorPlayerId,
            PlayerId = playerId,
            PlayerName = playerName
        }, cancellationToken);
    }
    
    public async Task<BasicOutcome> AcceptInvite(ulong instigatorPlayerId, CancellationToken cancellationToken)
    {
        return await PostAsync("party/invite/accept", new
        {
            InstigatorPlayerId = instigatorPlayerId
        }, cancellationToken);
    }
    
    public async Task<BasicOutcome> AcceptRequest(ulong instigatorPlayerId, ulong playerId, CancellationToken cancellationToken)
    {
        return await PostAsync("party/request/accept", new
        {
            InstigatorPlayerId = instigatorPlayerId,
            PlayerId = playerId
        }, cancellationToken);
    }
    
    public Task<BasicOutcome> RejectRequest(ulong instigatorPlayerId, ulong playerId, CancellationToken cancellationToken)
    {
        return CancelInvite(instigatorPlayerId, playerId, cancellationToken);
    }
    
    public async Task<BasicOutcome> PromoteToLeader(ulong instigatorPlayerId, ulong playerId, CancellationToken cancellationToken)
    {
        return await PostAsync("party/promote", new
        {
            InstigatorPlayerId = instigatorPlayerId,
            PlayerId = playerId
        }, cancellationToken);
    }
    
    public async Task<BasicOutcome> SetPartyRole(ulong instigatorPlayerId, string role, CancellationToken cancellationToken)
    {
        return await PostAsync("party/role", new
        {
            InstigatorPlayerId = instigatorPlayerId,
            Role = role
        }, cancellationToken);
    }
    
    public async Task<BasicOutcome> KickPartyMember(ulong instigatorPlayerId, ulong playerId, CancellationToken cancellationToken)
    {
        return await PostAsync("party/kick", new
        {
            InstigatorPlayerId = instigatorPlayerId,
            PlayerId = playerId
        }, cancellationToken);
    }

    private async Task<BasicOutcome> PostAsync(string path, object body, CancellationToken cancellationToken)
    {
        var url = Path.Combine(PveModBaseUrl.GetBaseUrl(), path);
        
        using var client = _httpClientFactory.CreateClient();

        var responseMessage = await client.PostAsync(
            url,
            new StringContent(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json"
            ),
            cancellationToken
        );

        var responseString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        return JsonConvert.DeserializeObject<BasicOutcome>(responseString);
    }
}