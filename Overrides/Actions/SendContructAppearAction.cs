using System;
using System.Threading.Tasks;
using Backend;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NQ;
using NQ.Interfaces;
using NQ.Visibility;
using Orleans;
using ConstructAppear = NQ.ConstructAppear;
using ConstructId = NQ.ConstructId;
using Vec3 = NQ.Vec3;

namespace Mod.DynamicEncounters.Overrides.Actions;

public class SendConstructAppearAction(IServiceProvider provider) : IModActionHandler
{
    public async Task HandleAction(ulong playerId, ModAction action)
    {
        var data = JsonConvert.DeserializeObject<ConstructAppearData>(action.payload);

        var pub = provider.GetRequiredService<IPub>();
        var message = await RetrieveConstructAppear(data.SelfConstructId, data.ConstructId, data.RadarId);

        await pub.NotifyPlayer(playerId, message!);
    }

    private async Task<NQutils.Messages.ConstructAppear?> RetrieveConstructAppear(
        ConstructId selfConstructId,
        ConstructId targetConstructId,
        ulong radarId
    )
    {
        var logger = provider.GetRequiredService<ILoggerFactory>()
            .CreateLogger<SendConstructAppearAction>();
        var orleans = provider.GetRequiredService<IClusterClient>();
        var internalClient = provider.GetRequiredService<Internal.InternalClient>();

        NQutils.Messages.ConstructAppear msg;

        try
        {
            var constructInfoGrain = orleans.GetConstructInfoGrain(targetConstructId);
            var constructInfo = await constructInfoGrain.Get();

            var radarData = await internalClient.RadarStartIfNeededAsync(
                new RadarRequest
                {
                    RadarId = radarId,
                    Space = true,
                    Range = 200000 * 20,
                    Location = NQ.RelativeLocation.From(new Vec3(), targetConstructId)
                }
            );

            foreach (var cid in radarData.ConstructIds)
            {
                logger.LogInformation("Found {C}", cid);
            }

            msg = new NQutils.Messages.ConstructAppear(
                new ConstructAppear
                {
                    camera = new CameraId { kind = CameraKind.Radar, id = radarId },
                    info = constructInfo
                }
            );

            logger.LogInformation("Send Construct Appear for {Name}", constructInfo.rData.name);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to send Appear");
            msg = null;
        }

        return msg;
    }

    public class ConstructAppearData
    {
        public ulong RadarId { get; set; }
        public ulong ConstructId { get; set; }
        public ulong SelfConstructId { get; set; }
    }
}