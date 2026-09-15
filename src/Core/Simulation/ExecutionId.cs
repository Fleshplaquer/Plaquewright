namespace Plaquewright.Core.Simulation;

public readonly record struct ExecutionId
    : IComparable<ExecutionId>
{
    public ulong Value { get; }

    public bool IsValid =>
        Value > 0UL;

    public ExecutionId(ulong value)
    {
        if (value == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "ExecutionId must be greater than zero.");
        }

        Value = value;
    }

    public int CompareTo(
        ExecutionId other)
    {
        return Value.CompareTo(
            other.Value);
    }

    public static bool operator <(
        ExecutionId left,
        ExecutionId right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >(
        ExecutionId left,
        ExecutionId right)
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