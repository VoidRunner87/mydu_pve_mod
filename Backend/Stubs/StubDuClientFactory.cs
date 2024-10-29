using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using BotLib.BotClient;
using BotLib.Generated;
using BotLib.Protocols;
using BotLib.Protocols.GrpcClient;
using BotLib.Protocols.Queuing;
using BotLib.Utils;
using Mod.DynamicEncounters.Common;
using NQ;
using Serilog;

namespace Mod.DynamicEncounters.Stubs;

public class StubDuClientFactory : IDuClientFactory
{
    private IQueuing Queuing;
    private readonly HttpClient _httpClient;

    public StubDuClientFactory(
        IHttpClientFactory httpClientFactory,
        IQueuing queuing)
    {
        Queuing = queuing;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<(DuClient, PlayerLoginState)> Connect(
        LoginInformations li,
        PlayerCreationInfo? creationInfos = null,
        bool allowExisting = false,
        GrpcVisibility.GrpcVisibilityConfig? grpcVisibilityConfig = null)
    {
        Console.WriteLine("Waiting in Queue");
        Log.ForContext<StubDuClientFactory>()
            .Information("Waiting in Queue");
        
        QueueingStreamedData queueingResponse = await Queuing.WaitInQueue(li);
        
#if !DEBUG
        queueingResponse.info.frontUri =
            EnvironmentVariableHelper.GetEnvironmentVarOrDefault(
                EnvironmentVariableNames.OverrideQueueingUrl,
                "queueing:9630"
            );
        queueingResponse.info.grpcInfo.address =
            EnvironmentVariableHelper.GetEnvironmentVarOrDefault(
                EnvironmentVariableNames.OverrideGrpcUrl,
                "10.5.0.5:9210"
            );
#endif
        
        Console.WriteLine($"Connection: {queueingResponse.info.grpcInfo.address}");
        
        GrpcVisibility grpc = await GrpcVisibility.Make(queueingResponse.info.grpcInfo, queueingResponse.token,
            grpcVisibilityConfig);
        
        Console.WriteLine($"Connection GPRC: {queueingResponse.info.grpcInfo.address}");
        Console.WriteLine($"Connection FRONT: {queueingResponse.info.frontUri}");
        Log.ForContext<StubDuClientFactory>()
            .Information("Connecting {username} to front `{front}` ", li, grpc);
        
        DuClient client = new DuClient(grpc, li, _httpClient, queueingResponse.info.frontUri);
        DuClient c1 = client;
        PlayerQueueingResponse req1 = new PlayerQueueingResponse();
        req1.username = li.ToString();
        req1.encryptionKey = "???";
        req1.JWToken = queueingResponse.token;
        CancellationToken ct1 = new CancellationToken();
        LoginResponseOrCreation loginresponse = await c1.PlayerLogin(req1, ct1);
        PlayerLoginState lr = loginresponse.optState;
        if (loginresponse.kind == LoginResponseKind.CreationNeeded)
        {
            DuClient c2 = client;
            PlayerCreationInfo req2 = creationInfos;
            if (req2 == null)
                req2 = new PlayerCreationInfo()
                {
                    colors = "0,0,0",
                    gender = NQRandom.Random.Next(2) == 0
                };
            CancellationToken ct2 = new CancellationToken();
            lr = await c2.PlayerCreationData(req2, ct2);

#if !DEBUG
            lr.itemBankUrl = lr.itemBankUrl.Replace("http://localhost:9630", Environment.GetEnvironmentVariable("QUEUEING"));
#endif
            Console.WriteLine($"Item Bank URL CREATION NEEDED: {lr.itemBankUrl}");
            
            await ItemBankGetter.InitOrUpdate(lr);
            client.updateWithLoginResponse(lr);
        }
        else
        {
            if (!allowExisting)
            {
                var interpolatedStringHandler = new StringBuilder();
                interpolatedStringHandler.Append(
                    "Connect: expecting a new client but player doesn't need creation: (player: ");
                interpolatedStringHandler.Append(loginresponse.optState.spawnState.playerId);
                interpolatedStringHandler.Append(" | name : ");
                interpolatedStringHandler.Append(loginresponse.optState.loginResponse.username);
                interpolatedStringHandler.Append(")");
                throw new Exception(interpolatedStringHandler.ToString());
            }
            
            Console.WriteLine($"OLD Item Bank URL NOT CREATION: {lr.itemBankUrl}");
            
#if !DEBUG
            lr.itemBankUrl = lr.itemBankUrl.Replace("http://localhost:9630", Environment.GetEnvironmentVariable("QUEUEING"));
#endif
            Console.WriteLine($"OVERRIDE Item Bank URL NOT CREATION: {lr.itemBankUrl}");

            await ItemBankGetter.InitOrUpdate(loginresponse.optState);
            client.updateWithLoginResponse(loginresponse.optState);
        }

        await grpc.StartHandle(
            new ActionBlock<IServerMessage>((Action<IServerMessage>)(msg => client.msgReceived(msg))),
            client.CancellationTokenSource.Token);
        (DuClient, PlayerLoginState) valueTuple = (client, lr);
        return valueTuple;
    }

    public void Disconnect()
    {
    }
}