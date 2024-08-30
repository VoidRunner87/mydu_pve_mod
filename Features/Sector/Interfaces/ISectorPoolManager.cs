using System.Collections.Generic;
using System.Threading.Tasks;
using BotLib.BotClient;
using Mod.DynamicEncounters.Features.Sector.Data;

namespace Mod.DynamicEncounters.Features.Sector.Interfaces;

public interface ISectorPoolManager
{
    Task<IEnumerable<SectorInstance>> GenerateSectorPool(SectorGenerationArgs args);

    Task LoadUnloadedSectors(Client client);

    Task ExecuteSectorCleanup(SectorGenerationArgs args);

    Task ActivateEnteredSectors(Client client);
}