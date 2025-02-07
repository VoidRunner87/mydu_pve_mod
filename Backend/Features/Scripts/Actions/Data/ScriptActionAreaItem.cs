using System.Numerics;
using Newtonsoft.Json;

namespace Mod.DynamicEncounters.Features.Scripts.Actions.Data;

public class ScriptActionAreaItem
{
    [JsonProperty]
    public string Type { get; set; } = "sphere";
    [JsonProperty]
    public float Radius { get; set; } = 200000;
    [JsonProperty]
    public float MinRadius { get; set; } = 100000;
    [JsonProperty]
    public float Height { get; set; } = 200000;
    [JsonProperty]
    public QuaternionItem Rotation { get; set; } = new();

    public class QuaternionItem
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; } = 1;

        public Quaternion ToQuaternion() => new() { X = X, Y = Y, Z = Z, W = W };
    }
}