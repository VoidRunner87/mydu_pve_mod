using Serilog.Core;
using Serilog.Events;

namespace Mod.DynamicEncounters.Common.Helpers;

public class ClassNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
        {
            var fullClassName = sourceContext.ToString().Trim('"');
            var className = fullClassName[(fullClassName.LastIndexOf('.') + 1)..];

            var classNameProperty = new LogEventProperty("SourceContext", new ScalarValue(className));
            logEvent.AddOrUpdateProperty(classNameProperty);
        }
    }
}