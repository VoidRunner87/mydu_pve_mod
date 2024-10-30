using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Commands.Interfaces;
using Mod.DynamicEncounters.Features.Commands.Repository;

namespace Mod.DynamicEncounters.Features.Commands;

public static class CommandsRegistration
{
    public static void RegisterCommands(this IServiceCollection services)
    {
        services.AddSingleton<IPendingCommandRepository, PendingCommandRepository>();
    }
}