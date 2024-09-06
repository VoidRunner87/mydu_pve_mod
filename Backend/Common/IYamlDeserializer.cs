namespace Mod.DynamicEncounters.Common;

public interface IYamlDeserializer
{
    T Deserialize<T>(string contents);
}