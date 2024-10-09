using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

public interface ITraitRepository
{
    Task<ITraitCollection> Get();
}