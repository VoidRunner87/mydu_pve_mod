namespace Mod.DynamicEncounters.Common.Interfaces;

public interface IYamlDeserializer
{
    T Deserialize<T>(string contents);
}