using Mod.DynamicEncounters.Common.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Mod.DynamicEncounters.Common.Services;

public class YamlDeserializer : IYamlDeserializer
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public T Deserialize<T>(string contents)
    {
        return _deserializer.Deserialize<T>(contents);
    }
}