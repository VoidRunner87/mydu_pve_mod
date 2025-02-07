using System.Numerics;

namespace Mod.DynamicEncounters.Helpers;

public static class SystemNumericsQuaternionHelpers
{
    public static Vector3 Forward(this Quaternion quaternion)
    {
        // Convert quaternion to rotation matrix
        var matrix = Matrix4x4.CreateFromQuaternion(quaternion);

        // Extract the forward vector
        var forwardVector = new Vector3(matrix.M12, matrix.M22, matrix.M32);

        return forwardVector;
    }
}