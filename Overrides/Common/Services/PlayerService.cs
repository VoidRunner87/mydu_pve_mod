using System;
using System.Threading.Tasks;
using Backend.Scenegraph;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Overrides.Common.Data;
using Mod.DynamicEncounters.Overrides.Common.Interfaces;

namespace Mod.DynamicEncounters.Overrides.Common.Services;

public class PlayerService(IServiceProvider provider) : IPlayerService
{
    private readonly IScenegraphAPI _sceneGraphApi = provider.GetRequiredService<IScenegraphAPI>();
    
    private readonly TemporaryMemoryCache<ulong, PlayerPosition?> _playerPosition =
        new(
            nameof(_playerPosition),
            TimeSpan.FromSeconds(5)
        );
    
    public async Task<PlayerPosition?> GetPlayerPosition(ulong playerId)
    {
        var position = await _sceneGraphApi.GetPlayerPosition(playerId);

        if (position == null)
        {
            return new PlayerPosition
            {
                Valid = false
            };
        }
        
        return new PlayerPosition
        {
            Valid = true,
            Position = position.position,
            ConstructId = position.constructId
        };
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