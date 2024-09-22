using NQ;

namespace Mod.DynamicEncounters.Common.Vector;

public static class LongVectorExtensions
{
    public static LongVector3 ToLongVector3(this Vec3 vec3) => new(vec3);
}