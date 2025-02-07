using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Events.Interfaces;
using Mod.DynamicEncounters.Features.Events.Repository;
using Mod.DynamicEncounters.Features.Events.Services;

namespace Mod.DynamicEncounters.Features.Events;

public static class EventRegistration
{
    public static void RegisterEvents(this IServiceCollection services)
    {
        services.AddSingleton<IEventRepository, EventRepository>();
        services.AddSingleton<IEventTriggerRepository, EventTriggerRepository>();
        services.AddSingleton<IEventService, EventService>();
    }
}