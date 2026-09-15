namespace Plaquewright.Core.Simulation;

public readonly record struct ScheduledSequence
    : IComparable<ScheduledSequence>
{
    public ulong Value { get; }

    public bool IsValid => Value > 0UL;

    public ScheduledSequence(ulong value)
    {
        if (value == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "Scheduled sequence must be greater than zero.");
        }

        Value = value;
    }

    public int CompareTo(ScheduledSequence other)
    {
        return Value.CompareTo(other.Value);
    }

    public static bool operator <(
        ScheduledSequence left,
        ScheduledSequence right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >(
        ScheduledSequence left,
        ScheduledSequence right)
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