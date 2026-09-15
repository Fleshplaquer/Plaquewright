namespace Plaquewright.Core.Simulation;

public readonly record struct SchedulerWave
    : IComparable<SchedulerWave>
{
    public static SchedulerWave Initial => new(0u);

    public uint Value { get; }

    public SchedulerWave(uint value)
    {
        Value = value;
    }

    public SchedulerWave Next()
    {
        return new SchedulerWave(
            checked(Value + 1u));
    }

    public int CompareTo(SchedulerWave other)
    {
        return Value.CompareTo(other.Value);
    }

    public static bool operator <(
        SchedulerWave left,
        SchedulerWave right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >(
        SchedulerWave left,
        SchedulerWave right)
    {
        return left.Value > right.Value;
    }

    public static bool operator <=(
        SchedulerWave left,
        SchedulerWave right)
    {
        return left.Value <= right.Value;
    }

    public static bool operator >=(
        SchedulerWave left,
        SchedulerWave right)
    {
        return left.Value >= right.Value;
    }
}