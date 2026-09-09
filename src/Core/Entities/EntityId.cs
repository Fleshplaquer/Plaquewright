namespace Idler.Core.Entities;

public readonly record struct EntityId
    : IComparable<EntityId>
{
    public ulong Value { get; }

    public bool IsValid =>
        Value > 0UL;

    public EntityId(ulong value)
    {
        if (value == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "EntityId must be greater than zero.");
        }

        Value = value;
    }

    public int CompareTo(
        EntityId other)
    {
        return Value.CompareTo(
            other.Value);
    }

    public static bool operator <(
        EntityId left,
        EntityId right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >(
        EntityId left,
        EntityId right)
    {
        return left.Value > right.Value;
    }

    public override string ToString()
    {
        return IsValid
            ? Value.ToString()
            : "<invalid>";
    }
}