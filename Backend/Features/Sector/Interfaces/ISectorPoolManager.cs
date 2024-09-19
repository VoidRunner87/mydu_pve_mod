using System;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.Sector.Data;
using NQ;

namespace Mod.DynamicEncounters.Features.Sector.Interfaces;

public interface ISectorPoolManager
{
    Task GenerateSectors(SectorGenerationArgs args);

    Task LoadUnloadedSectors();

    Task ExecuteSectorCleanup(SectorGenerationArgs args);
    Task SetExpirationFromNow(Vec3 sector, TimeSpan span);

    Task ActivateEnteredSectors();
}