using System;
using System.Threading.Tasks;

namespace Mod.DynamicEncounters;

public class HealthCheckLoop : ModBase
{
    public override async Task Loop()
    {
        while (true)
        {
            Console.WriteLine("Live");
            await Task.Delay(10000);
        }
    }
}