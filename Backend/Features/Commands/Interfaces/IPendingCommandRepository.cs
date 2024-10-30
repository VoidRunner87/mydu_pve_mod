using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Commands.Data;

namespace Mod.DynamicEncounters.Features.Commands.Interfaces;

public interface IPendingCommandRepository
{
    Task<IEnumerable<CommandItem>> QueryAsync(DateTime afterDateTime);
}