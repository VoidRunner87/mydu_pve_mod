using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BotLib.BotClient;
using Mod.DynamicEncounters.Features.Sector.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Sector.Interfaces;

public interface ISectorPoolManager
{
    Task<IEnumerable<SectorInstance>> GenerateSectorPool(SectorGenerationArgs args);

    Task LoadUnloadedSectors(Client client);

    Task ExecuteSectorCleanup(Client client, SectorGenerationArgs args);
    Task SetExpirationFromNow(Vec3 sector, TimeSpan span);

    Task ActivateEnteredSectors(Client client);
}