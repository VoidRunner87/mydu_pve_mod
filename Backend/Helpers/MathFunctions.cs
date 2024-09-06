namespace Mod.DynamicEncounters.Helpers;

public static class MathFunctions
{
    public static double Lerp(double min, double max, double alpha)
    {
        return min + (max - min) * alpha;
    }
}