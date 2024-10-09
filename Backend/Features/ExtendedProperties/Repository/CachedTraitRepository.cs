using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Repository;

public class CachedTraitRepository(ITraitRepository repository) : ITraitRepository
{
    private static ITraitCollection? _traitCollection;
    
    public async Task<ITraitCollection> Get()
    {
        if (_traitCollection == null)
        {
            _traitCollection = await repository.Get();
        }

        return _traitCollection!;
    }

    public static void Clear()
    {
        _traitCollection = null;
    }
}