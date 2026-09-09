namespace Idler.Core.Combat;

public readonly record struct DamageExecutionId
    : IComparable<DamageExecutionId>
{
    public ulong Value { get; }

    public bool IsValid =>
        Value != 0UL;

    public DamageExecutionId(
        ulong value)
    {
        if (value == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Damage execution ID must be greater than zero.");
        }

        Value =
            value;
    }

    public int CompareTo(
        DamageExecutionId other)
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
        DamageExecutionId left,
        DamageExecutionId right)
    {
        return left.Value <
            right.Value;
    }

    public static bool operator >(
        DamageExecutionId left,
        DamageExecutionId right)
    {
        return left.Value >
            right.Value;
    }

    public static bool operator <=(
        DamageExecutionId left,
        DamageExecutionId right)
    {
        return left.Value <=
            right.Value;
    }

    public static bool operator >=(
        DamageExecutionId left,
        DamageExecutionId right)
    {
        return left.Value >=
            right.Value;
    }
}