using System;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Mod.DynamicEncounters.Database.Interfaces;
using Mod.DynamicEncounters.Features.Common.Data;

namespace Mod.DynamicEncounters.Features.Common.Interfaces;

public class ErrorRepository(IServiceProvider provider) : IErrorRepository
{
    private readonly IPostgresConnectionFactory _factory = 
        provider.GetRequiredService<IPostgresConnectionFactory>();

    public async Task AddAsync(ErrorItem item)
    {
        using var db = _factory.Create();
        db.Open();

        await db.ExecuteAsync(
            """
            INSERT INTO mod_error (id, type, subtype, error) 
            VALUES (@id, @type, @subtype, @error)
            """,
            new
            {
                id = item.Id,
                type = item.Type,
                subtype = item.SubType,
                error = item.Error
            }
        );
    }
}