using System;
using Mod.DynamicEncounters.Common.Repository;
using Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Repository;

public class ConstructDefinitionMemoryRepository(IServiceProvider provider) : BaseMemoryRepository<string, IPrefab>(provider);