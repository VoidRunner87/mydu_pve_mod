using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.Interfaces;

public interface IFeatureWriterService
{
    Task EnableStarterContentFeaturesAsync();
}