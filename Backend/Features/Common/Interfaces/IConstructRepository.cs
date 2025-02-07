using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Common.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public interface IConstructRepository
{
    Task<IEnumerable<ConstructItem>> FindByKind(ConstructKind kind);

    Task<IEnumerable<ConstructItem>> FindAsteroids();
    
    Task<IEnumerable<ConstructItem>> FindOnlinePlayerConstructs();
    Task<IEnumerable<ConstructItem>> FindActiveNpcConstructs();
}