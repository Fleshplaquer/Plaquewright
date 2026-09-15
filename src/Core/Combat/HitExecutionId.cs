namespace Plaquewright.Core.Combat;

public readonly record struct HitExecutionId
    : IComparable<HitExecutionId>
{
    public ulong Value { get; }

    public bool IsValid =>
        Value != 0UL;

    public HitExecutionId(
        ulong value)
    {
        if (value == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Hit execution ID must be greater than zero.");
        }

        Value =
            value;
    }

    public int CompareTo(
        HitExecutionId other)
    {
        return Value.CompareTo(
            other.Value);
    }

    public override string ToString()
    {
        return IsValid
            ? Value.ToString()
            : "<invalid>";
    }

    public static bool operator <(
        HitExecutionId left,
        HitExecutionId right)
    {
        return left.Value <
            right.Value;
    }

    public static bool operator >(
        HitExecutionId left,
        HitExecutionId right)
    {
        return left.Value >
            right.Value;
    }

    public static bool operator <=(
        HitExecutionId left,
        HitExecutionId right)
    {
        return left.Value <=
            right.Value;
    }

    public static bool operator >=(
        HitExecutionId left,
        HitExecutionId right)
    {
        return left.Value >=
            right.Value;
    }
}