using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Features.Commands.Interfaces;
using Mod.DynamicEncounters.Features.Commands.Repository;
using Mod.DynamicEncounters.Features.Commands.Services;

namespace Mod.DynamicEncounters.Features.Commands;

public static class CommandsRegistration
{
    public static void RegisterCommands(this IServiceCollection services)
    {
        services.AddSingleton<IPendingCommandRepository, PendingCommandRepository>();
        services.AddSingleton<INpcKillsCommandHandler, NpcKillsCommandHandler>();
        services.AddSingleton<IWarpAnchorCommandHandler, WarpAnchorCommandHandler>();
        services.AddSingleton<IOpenPlayerBoardCommandHandler, OpenPlayerBoardCommandHandler>();
    }
}