namespace NpcMovementLib.Data;

/// <summary>
/// Strongly-typed value object representing a unique construct identifier.
/// Wraps the raw <see cref="ulong"/> ID used by the game engine to prevent
/// primitive obsession and accidental misuse (e.g., passing a player ID
/// where a construct ID is expected).
/// </summary>
/// <remarks>
/// <para>
/// In the game backend, construct IDs are 64-bit unsigned integers assigned
/// at creation time and stored in the <c>public.construct</c> table's <c>id</c>
/// column. They are used across the entire NPC pipeline — radar scanning,
/// transform lookups, velocity queries, and server updates.
/// </para>
/// <para>
/// This struct provides an <see langword="implicit"/> conversion <b>to</b>
/// <see cref="ulong"/> so it can be passed transparently to APIs that expect
/// a raw ID (e.g., Orleans grain lookups, SQL parameters). The reverse
/// conversion is <see langword="explicit"/> to force callers to be intentional
/// when wrapping a raw value.
/// </para>
/// <para>
/// As a <see langword="readonly record struct"/>, <see cref="ConstructId"/>
/// is stack-allocated, implements value equality, and is safe to use as a
/// dictionary key or in hash sets.
/// </para>
/// </remarks>
/// <param name="Value">The underlying 64-bit construct identifier from the game engine.</param>
/// <example>
/// <code>
/// var id = new ConstructId(42);
/// ulong raw = id;                  // implicit conversion to ulong
/// var back = (ConstructId)raw;     // explicit conversion from ulong
/// </code>
/// </example>
public readonly record struct ConstructId(ulong Value)
{
    /// <summary>
    /// Implicitly converts a <see cref="ConstructId"/> to its underlying <see cref="ulong"/> value.
    /// This allows seamless interop with game-engine APIs that accept raw IDs.
    /// </summary>
    /// <param name="id">The construct identifier to convert.</param>
    public static implicit operator ulong(ConstructId id) => id.Value;

    /// <summary>
    /// Explicitly converts a raw <see cref="ulong"/> to a <see cref="ConstructId"/>.
    /// The conversion is explicit to ensure callers consciously wrap raw values,
    /// reducing the risk of accidentally assigning unrelated IDs.
    /// </summary>
    /// <param name="value">The raw 64-bit identifier to wrap.</param>
    public static explicit operator ConstructId(ulong value) => new(value);

    /// <summary>
    /// Returns the decimal string representation of the underlying ID.
    /// </summary>
    public override string ToString() => Value.ToString();
}
