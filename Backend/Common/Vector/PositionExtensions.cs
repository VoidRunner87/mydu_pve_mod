using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using NQ;

namespace Mod.DynamicEncounters.Common.Vector;

public static class PositionExtensions
{
    public static Vec3 PositionToVec3(this string position)
    {
        var replacedString = position.Replace("::pos{", string.Empty)
            .Replace("}", string.Empty);

        var pieces = replacedString.Split(',', StringSplitOptions.RemoveEmptyEntries);

        if (pieces.Length != 5)
        {
            throw new ArgumentException(
                $"Invalid DU Position format. Example: ::pos{{0,0,5236583.0860,-9051901.5198,-857517.7448}}. Param = {position}", 
                nameof(position)
            );
        }
        
        var queue = new Queue<string>(pieces);
        queue.Dequeue();
        queue.Dequeue();

        var x = double.Parse(queue.Dequeue());
        var y = double.Parse(queue.Dequeue());
        var z = double.Parse(queue.Dequeue());

        return new Vec3
        {
            x = x,
            y = y,
            z = z
        };
    }

    public static string Vec3ToPosition(this Vector3 vector3, int constructId = 0, int precision = 0)
    {
        var sb = new StringBuilder();

        sb.Append("::pos{0,");
        sb.Append(constructId);
        sb.Append(',');
        sb.Append(vector3.X.ToString($"F{precision}"));
        sb.Append(',');
        sb.Append(vector3.Y.ToString($"F{precision}"));
        sb.Append(',');
        sb.Append(vector3.Z.ToString($"F{precision}"));
        sb.Append('}');
        
        return sb.ToString();
    }
}