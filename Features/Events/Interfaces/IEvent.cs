using System;
using Newtonsoft.Json.Linq;

namespace Mod.DynamicEncounters.Features.Events.Interfaces;

public interface IEvent
{
    Guid Id { get; }
    string Name { get; }
    object Data { get; }
    int Value { get; }
    ulong? PlayerId { get; }

    JToken DataAsJToken();
}