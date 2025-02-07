using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Overrides.Common.Interfaces;
using NQ;
using NQutils.Sql;
using PlayerPosition = Mod.DynamicEncounters.Overrides.Common.Data.PlayerPosition;

namespace Mod.DynamicEncounters.Overrides.Common.Services;

public class PlayerService(IServiceProvider provider) : IPlayerService
{
    private readonly ISql _sql = provider.GetRequiredService<ISql>();

    private readonly TemporaryMemoryCache<ulong, PlayerPosition?> _playerPosition =
        new(
            nameof(_playerPosition),
            TimeSpan.FromSeconds(5)
        );

    public async Task<PlayerPosition> GetPlayerPosition(ulong playerId)
    {
        await Task.Yield();
        
        var result = _sql.Q(
            """
            SELECT 
                P.position_x, 
                P.position_y, 
                P.position_z, 
                P.construct_id
            FROM public.player P
            WHERE P.id = @1
            """,
            (long)playerId
        );

        return await result.First(row =>
        {
            if (row == null)
            {
                return new PlayerPosition
                {
                    Valid = false
                };
            }

            var (positionX, positionY, positionZ, constructId) = row.GetTuple<(double, double, double, ulong)>();

            return new PlayerPosition
            {
                Valid = true,
                Position = new Vec3
                {
                    x = positionX,
                    y = positionY,
                    z = positionZ
                },
                ConstructId = constructId
            };
        });
    }

    public Task<PlayerPosition?> GetPlayerPositionCached(ulong playerId)
    {
        return _playerPosition.TryGetOrSetValue(
            playerId,
            () => GetPlayerPosition(playerId),
            pos => pos == null
        );
    }
}