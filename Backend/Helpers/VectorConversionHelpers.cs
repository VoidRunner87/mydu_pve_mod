using System.Numerics;
using NQ;

namespace Mod.DynamicEncounters.Helpers;

public static class VectorConversionHelpers
{
    public static Vector3 ToFVector3(this Vec3 v) 
        => new Vector3((float)v.x, (float)v.y, (float)v.z);
    
    public static MathNet.Numerics.LinearAlgebra.Vector<double> ToMnVector3(this Vec3 v) 
        => MathNet.Numerics.LinearAlgebra.Vector<double>.Build.DenseOfArray([v.x, v.y, v.z]);

    public static Vec3 ToNqVec3(this MathNet.Numerics.LinearAlgebra.Vector<double> v) 
        => new() { x = v[0], y = v[1], z = v[2] };
}