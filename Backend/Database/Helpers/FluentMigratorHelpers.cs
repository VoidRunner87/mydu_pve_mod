using FluentMigrator.Builders;
using FluentMigrator.Infrastructure;

namespace Mod.DynamicEncounters.Database.Helpers;

public static class FluentMigratorHelpers
{
    public static TNext AsDateTimeUtc<TNext>(this IColumnTypeSyntax<TNext> col) where TNext : IFluentSyntax
    {
        return col.AsCustom("timestamp with time zone");
    }
    
}