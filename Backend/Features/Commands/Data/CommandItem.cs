using System;

namespace Mod.DynamicEncounters.Features.Commands.Data;

public class CommandItem
{
    public long Id { get; set; }
    public ulong PlayerId { get; set; }
    public string Message { get; set; }
    public DateTime Date { get; set; }
}