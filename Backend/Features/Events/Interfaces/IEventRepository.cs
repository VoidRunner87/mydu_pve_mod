using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.Events.Interfaces;

public interface IEventRepository
{
    Task AddAsync(IEvent @event);

    Task<double> GetSumAsync(string eventName, ulong? playerId);
}