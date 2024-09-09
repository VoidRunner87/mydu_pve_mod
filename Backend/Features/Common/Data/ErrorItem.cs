using System;

namespace Mod.DynamicEncounters.Features.Common.Data;

public class ErrorItem(string type, string subType, string error)
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Type { get; } = type;
    public string SubType { get; } = subType;
    public string Error { get; } = error;
}