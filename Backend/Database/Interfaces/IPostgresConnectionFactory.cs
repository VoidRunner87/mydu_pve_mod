using System.Data;

namespace Mod.DynamicEncounters.Database.Interfaces;

public interface IPostgresConnectionFactory
{
    IDbConnection Create();
}