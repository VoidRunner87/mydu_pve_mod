namespace Mod.DynamicEncounters.Features.Common.Data;

public readonly struct Quanta(long value)
{
    public long Value { get; } = value;
    
    public static implicit operator long(Quanta quanta) => quanta.Value;
    public static implicit operator Quanta(long value) => new(value);
    public static implicit operator Quanta(double value) => new((long)value);

    public static Quanta operator +(Quanta q1, Quanta q2)
    {
        return new Quanta(q1.Value + q2.Value);
    }
    
    public static Quanta operator -(Quanta q1, Quanta q2)
    {
        return new Quanta(q1.Value - q2.Value);
    }
}