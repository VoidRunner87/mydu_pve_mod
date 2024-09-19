using System.Collections.Generic;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Data;

public class ConstructHandleProperties
{
    public string FactionId { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public List<string> Behaviors { get; set; } = new();
}