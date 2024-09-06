namespace Mod.DynamicEncounters.Features.Scripts.Actions.Data;

public class ScriptActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public ScriptActionResult WithMessage(string message)
    {
        Message = message;
        
        return this;
    }

    public static ScriptActionResult Successful() => new ScriptActionResult { Success = true };
    public static ScriptActionResult Failed() => new ScriptActionResult { Success = false };
}