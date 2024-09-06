using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.Events.Interfaces;

public interface IEventService
{
    Task PublishAsync(IEvent @event);
}