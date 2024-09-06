using System.Reflection;
using System.Threading.Tasks;

namespace Mod.DynamicEncounters.Common;

public static class ModRunner
{
    public static Task StartModTask(Assembly executingAssembly, string typeName)
    {
        var initializerModType = executingAssembly.GetType(typeName);
        var constructor = initializerModType!.GetConstructor([]);
        var instance = constructor!.Invoke([]);

        var startMethod = initializerModType.GetMethod("Start");
        return (Task)startMethod!.Invoke(instance, [])!;
    } 
}