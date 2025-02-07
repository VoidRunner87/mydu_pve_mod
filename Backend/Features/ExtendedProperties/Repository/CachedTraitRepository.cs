using System.Collections.Concurrent;
using System.Threading.Tasks;
using Mod.DynamicEncounters.Features.ExtendedProperties.Interfaces;

namespace Mod.DynamicEncounters.Features.ExtendedProperties.Repository;

public class CachedTraitRepository(ITraitRepository repository) : ITraitRepository
{
    private static ITraitCollection? _traitCollection;
    private static readonly ConcurrentDictionary<string, ITraitCollection> ElementTraitCollection = new();
    
    public async Task<ITraitCollection> Get()
    {
        if (_traitCollection == null)
        {
            _traitCollection = await repository.Get();
        }

        return _traitCollection!;
    }

    public async Task<ITraitCollection> GetElementTraits(string elementTypeName)
    {
        if (!ElementTraitCollection.TryGetValue(elementTypeName, out var traitCollection))
        {
            var result = await repository.GetElementTraits(elementTypeName);
            ElementTraitCollection.TryAdd(elementTypeName, result);

            return result;
        }
       
        return traitCollection;
    }

    public static void Clear()
    {
        _traitCollection = null;
        ElementTraitCollection.Clear();
    }
}