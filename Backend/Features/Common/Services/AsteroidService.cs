using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Interfaces;

namespace Mod.DynamicEncounters.Features.Common.Services;

public class AsteroidService(IServiceProvider provider) : IAsteroidService
{
    private readonly IPostgresConnectionFactory _factory = provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task HideFromDsatListAsync(ulong constructId)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            DELETE FROM public.asteroid WHERE construct_id = @construct_id
            """,
            new
            {
                construct_id = (long)constructId
            }
        );
    }
}