using System;
using System.Net.Http;
using NQ;

namespace Mod.DynamicEncounters.Features.VoxelService.Data;

public class QueryRandomPointOutcome
{
    public Vec3 LocalPosition { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static QueryRandomPointOutcome FoundPosition(Vec3 localPosition)
        => new() { Success = true, LocalPosition = localPosition };
    
    public static QueryRandomPointOutcome Disabled()
        => new() { Success = false, Message = "PVE Voxel Service is Disabled" };

    public static QueryRandomPointOutcome Failed(HttpResponseMessage responseMessage)
        => new() { Success = false, Message = $"Failed to retrieve point: HTTP: {responseMessage.StatusCode}" };
    
    public static QueryRandomPointOutcome Failed(Exception exception)
        => new() { Success = false, Message = $"Failed to retrieve point: Exception: {exception.Message}" };
}