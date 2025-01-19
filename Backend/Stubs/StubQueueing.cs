using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BotLib.Protocols.Queuing;
using Mod.DynamicEncounters.Common.Data;
using Mod.DynamicEncounters.Common.Helpers;
using Newtonsoft.Json;
using NQ;
using NQutils.Exceptions;
using NQutils.Net;
using Serilog;

namespace Mod.DynamicEncounters.Stubs;

public class StubRealQueuing : IQueuing
{
    private readonly HttpClient _httpClient;
    private readonly string _address;
    private SemaphoreSlim _lock = new(1);
    private ProtocolInfo? _protocol;

    public StubRealQueuing(string address, HttpClient httpClient, ProtocolInfo? protocol = null)
    {
        _address = address;
        _protocol = protocol;
        _httpClient = httpClient;
    }

    public static async Task<RealQueuing> Make(string address, HttpClient httpClient)
    {
        return new RealQueuing(address, httpClient, await RealQueuing.GetProtocolInfo(address, httpClient));
    }

    public static async Task<ProtocolInfo> GetProtocolInfo(string address, HttpClient httpClient)
    {
        ProtocolInfo protocolInfo;
        try
        {
            ProtocolInfo protocol = await httpClient.Get<ProtocolInfo>(address + "/public/auth/protocol",
                callerName: nameof(GetProtocolInfo));
            if (protocol.protocolSignature != ProtocolVersion.protocolSignature)
            {
                TextWriter error = Console.Error;
                var interpolatedStringHandler = new StringBuilder();
                interpolatedStringHandler.Append("Warning: protocol mismatch: queue protocol: `");
                interpolatedStringHandler.Append(protocol.protocolSignature);
                interpolatedStringHandler.Append("` bot protocol `");
                interpolatedStringHandler.Append(ProtocolVersion.protocolSignature);
                interpolatedStringHandler.Append("`");
                string stringAndClear = interpolatedStringHandler.ToString();
                await error.WriteLineAsync(stringAndClear);
            }

            protocolInfo = protocol;
        }
        catch (Exception ex)
        {
            TextWriter error = Console.Error;
            var interpolatedStringHandler = new StringBuilder(62, 2);
            interpolatedStringHandler.Append("Can't connect to queuing service at ");
            interpolatedStringHandler.Append(address);
            interpolatedStringHandler.Append("/public/auth/protocol : \n ");
            interpolatedStringHandler.Append(ex);
            string stringAndClear = interpolatedStringHandler.ToString();
            error.WriteLine(stringAndClear);
            throw;
        }

        return protocolInfo;
    }

    private async Task<ProtocolInfo> GetProtocolInfo()
    {
        try
        {
            await _lock.WaitAsync();
            _protocol = await RealQueuing.GetProtocolInfo(_address, _httpClient);
        }
        finally
        {
            _lock.Release();
        }

        return _protocol;
    }

    public async Task<QueueingStreamedData> WaitInQueue(LoginInformations li)
    {
        var response = await _httpClient.PostAsync(_address + "/public/auth/bot",
            HTTPClientExtensions.SerializeContent(li.MakeQueuingRequest(await GetProtocolInfo()), false));
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var s = await response.Content.ReadAsStringAsync();
            try
            {
                throw new BusinessException(NQutils.Serialization.Serialization.Deserialize<Error>(
                    (ReadOnlyMemory<byte>)Encoding.UTF8.GetBytes(s), NQutils.Serialization.Serialization.Format.JSON));
            }
            catch (Exception ex) when (!(ex is BusinessException))
            {
                var interpolatedStringHandler = new StringBuilder(68, 4);
                interpolatedStringHandler.Append("Error while connecting to queuing service : ");
                interpolatedStringHandler.Append(_address);
                interpolatedStringHandler.Append("/public/auth/token > ");
                interpolatedStringHandler.Append(response.StatusCode);
                interpolatedStringHandler.Append(" ");
                interpolatedStringHandler.Append(response.StatusCode);
                interpolatedStringHandler.Append(" \n");
                interpolatedStringHandler.Append(s);

                throw new Exception(interpolatedStringHandler.ToString());
            }
        }

        response.EnsureSuccessStatusCode();
        switch (response.Content.Headers.ContentType?.MediaType)
        {
            case "application/json":
                var data = JsonConvert.DeserializeObject<QueueingStreamedData>(
                    await response.Content.ReadAsStringAsync());

                Console.WriteLine($"GRPC Info 1: {data.info.grpcInfo.address}");

                if (EnvironmentVariableHelper.IsProduction())
                {
                    data.info.frontUri =
                        EnvironmentVariableHelper.GetEnvironmentVarOrDefault(
                            EnvironmentVariableNames.OverrideQueueingUrl,
                            "queueing:9630"
                        );
                    data.info.grpcInfo.address =
                        EnvironmentVariableHelper.GetEnvironmentVarOrDefault(
                            EnvironmentVariableNames.OverrideGrpcUrl,
                            "10.5.0.5:9210"
                        );
                }

                return data;
            case "text/event-stream":
                var eventStreamData = await WaitInQueue(await response.Content.ReadAsStreamAsync());

                Console.WriteLine($"GRPC Info 2: {eventStreamData.info.grpcInfo.address}");

                if (EnvironmentVariableHelper.IsProduction())
                {
                    eventStreamData.info.frontUri =
                        EnvironmentVariableHelper.GetEnvironmentVarOrDefault(
                            EnvironmentVariableNames.OverrideQueueingUrl,
                            "queueing:9630"
                        );
                    eventStreamData.info.grpcInfo.address =
                        EnvironmentVariableHelper.GetEnvironmentVarOrDefault(
                            EnvironmentVariableNames.OverrideGrpcUrl,
                            "10.5.0.5:9210"
                        );
                }

                return eventStreamData;
            default:
                throw new Exception($"Unknown content type in queuing response: {response.Content.Headers.ContentType}");
        }
    }

    private async Task<QueueingStreamedData> WaitInQueue(Stream dataStream)
    {
        var sr = new StreamReader(dataStream, Encoding.UTF8, false, 1000, false);
        var queueingStreamedData1 = new QueueingStreamedData();

        do
        {
            var str = await sr.ReadLineAsync();
            switch (str)
            {
                case null:
                    throw new Exception("end of stream");
                case "":
                    Console.WriteLine("Empty");
                    continue;
                default:
                    if (!str.StartsWith("data:"))
                    {
                        Console.WriteLine("Bad queuing data : " + str);
                        Log.Error("Bad queuing data : " + str);
                        continue;
                    }

                    Console.WriteLine("Default");

                    queueingStreamedData1 = JsonConvert.DeserializeObject<QueueingStreamedData>(str[5..]);
                    continue;
            }
        } while (queueingStreamedData1.queueIndex != 0 || queueingStreamedData1.token == "");

        var queueingStreamedData2 = queueingStreamedData1;

        return queueingStreamedData2;
    }
}