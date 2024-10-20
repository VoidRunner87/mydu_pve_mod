namespace Mod.DynamicEncounters.Features.Common.Data;

public class BlueprintSanitationResult(bool success, byte[] bytes, string message)
{
    public bool Success { get; } = success;

    public byte[] BlueprintBytes { get; } = bytes;

    public string Message { get; } = message;

    public static BlueprintSanitationResult Failed(string msg) => new(false, [], msg);

    public static BlueprintSanitationResult Succeeded(byte[] bp) => new(true, bp, string.Empty);
}