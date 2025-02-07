using System;
using NQ;

namespace Mod.DynamicEncounters.Features.Warp.Data;

public class CreateWarpAnchorOutcome
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }

    public ConstructId WarpAnchorConstructId { get; set; }
    public string WarpAnchorConstructName { get; set; } = string.Empty;
    public Vec3 WarpAnchorPosition { get; set; }

    public static CreateWarpAnchorOutcome WarpAnchorCreated(
        ConstructId constructId,
        string constructName,
        Vec3 warpAnchorPosition
    ) => new()
    {
        WarpAnchorConstructId = constructId,
        WarpAnchorConstructName = constructName,
        WarpAnchorPosition = warpAnchorPosition,
        Success = true,
        Message = "Warp anchor created"
    };

    public static CreateWarpAnchorOutcome Failed(string message, Exception exception)
        => new() { Message = message, Exception = exception };

    public static CreateWarpAnchorOutcome InvalidElementTypeName()
        => new() { Message = "Invalid Element" };

    public static CreateWarpAnchorOutcome ElementDoesNotHaveSuperCruise(string elementTypeName)
        => new() { Message = $"{elementTypeName} does not have Supercruise" };

    public static CreateWarpAnchorOutcome OnCooldown(TimeSpan timeSpan)
        => new() { Message = $"Warp Anchor is cooling down: {timeSpan.TotalSeconds}s" };

    public static CreateWarpAnchorOutcome InvalidPlayerPosition()
        => new() { Message = "Invalid Player Position" };

    public static CreateWarpAnchorOutcome MustBePilotingConstruct()
        => new() { Message = "You must pilot the construct to create a warp anchor" };

    public static CreateWarpAnchorOutcome MissingDriveUnit()
        => new() { Message = "Construct needs a Warp Drive or Supercruise Drive" };

    public static CreateWarpAnchorOutcome TooCloseToAPlanet()
        => new() { Message = "Gravitational anomaly detected on target destination" };

    public static CreateWarpAnchorOutcome InvalidWaypoint()
        => new() { Message = "Invalid waypoint" };

    public static CreateWarpAnchorOutcome InvalidPosition()
        => new() { Message = "Invalid position" };

    public static CreateWarpAnchorOutcome InvalidDistance()
        => new() { Message = "Invalid distance" };

    public static CreateWarpAnchorOutcome InvalidDriveUnit()
        => new() { Message = "Invalid Drive Unit" };
}