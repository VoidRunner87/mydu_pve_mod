using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.NQ.Interfaces;
using Mod.DynamicEncounters.Helpers;
using NQ;
using NQ.Interfaces;

namespace Mod.DynamicEncounters.Features.NQ.Services;

public class PlayerService(IServiceProvider provider) : IPlayerService
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task GrantPlayerTitleAsync(ulong playerId, string title)
    {
        using var db = _factory.Create();
        db.Open();

        var count = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(0) FROM public.player_title WHERE player_id = @playerId AND title = @title",
            new
            {
                playerId = (long)playerId, 
                title
            }
        );

        if (count > 0)
        {
            return;
        }

        await db.ExecuteAsync(
            """
            INSERT INTO public.player_title (player_id, title) VALUES(@playerId, @title)          
            """,
            new
            {
                playerId = (long)playerId,
                title
            }
        );

        var orleans = provider.GetOrleans();
        var playerNotification = orleans.GetNotificationGrain(playerId);
        await playerNotification.AddNewNotification(
            new NotificationMessage
            {
                category = NotificationCategory.PvEMission,
                notificationCode = EnumNotificationCode.PvEMissionCompleted,
                parameters = new List<NotificationParameter>
                {
                    new NotificationParameter
                    {
                        
                    }
                }
            }
        );
    }
}