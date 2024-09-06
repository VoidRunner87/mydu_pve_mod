using System.Data;
using Mod.DynamicEncounters.Database.Interfaces;
using Npgsql;

namespace Mod.DynamicEncounters.Database.Services;

public class PostgresConnectionFactory : IPostgresConnectionFactory
{
    public IDbConnection Create()
    {
        return new NpgsqlConnection(NQutils.Config.Config.Instance.postgres.ConnectionString());
    }
}