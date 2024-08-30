namespace Mod.DynamicEncounters.Features.Scripts.Actions.Interfaces;

public interface IHasKey<out T>
{
    T GetKey();
}