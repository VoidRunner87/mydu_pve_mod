using System;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Repository;

public class ScriptActionMemoryRepository(IServiceProvider provider) : BaseMemoryRepository<string, IScriptAction>(provider);