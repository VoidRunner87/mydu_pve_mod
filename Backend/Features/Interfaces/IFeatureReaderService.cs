using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Features.Interfaces;

public interface IFeatureReaderService
{
    Task<bool> GetBoolValueAsync(string name, bool defaultValue);
    Task<bool> GetEnabledValue<T>(bool defaultValue);
    Task<int> GetIntValueAsync(string name, int defaultValue);
    Task<string> GetStringValueAsync(string name, string defaultValue);
}