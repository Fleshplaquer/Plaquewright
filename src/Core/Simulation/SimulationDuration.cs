namespace Plaquewright.Core.Simulation;

public readonly record struct SimulationDuration
    : IComparable<SimulationDuration>
{
    public const long MicrosecondsPerMillisecond = 1_000L;
    public const long MicrosecondsPerSecond = 1_000_000L;

    public static SimulationDuration Zero => new(0L);

    public long Microseconds { get; }

    public SimulationDuration(long microseconds)
    {
        if (microseconds < 0L)
        {
            throw new ArgumentOutOfRangeException(
                nameof(microseconds),
                microseconds,
                "Simulation duration cannot be negative.");
        }

        Microseconds = microseconds;
    }

    public static SimulationDuration FromMicroseconds(
        long microseconds)
    {
        return new SimulationDuration(microseconds);
    }

    public static SimulationDuration FromMilliseconds(
        long milliseconds)
    {
        if (milliseconds < 0L)
        {
            throw new ArgumentOutOfRangeException(
                nameof(milliseconds),
                milliseconds,
                "Simulation duration cannot be negative.");
        }

        return new SimulationDuration(
            checked(
                milliseconds *
                MicrosecondsPerMillisecond));
    }

    public static SimulationDuration FromSeconds(
        long seconds)
    {
        if (seconds < 0L)
        {
            throw new ArgumentOutOfRangeException(
                nameof(seconds),
                seconds,
                "Simulation duration cannot be negative.");
        }

        return new SimulationDuration(
            checked(
                seconds *
                MicrosecondsPerSecond));
    }

    public int CompareTo(
        SimulationDuration other)
    {
        return Microseconds.CompareTo(
            other.Microseconds);
    }

    public static SimulationDuration operator +(
        SimulationDuration left,
        SimulationDuration right)
    {
        return new SimulationDuration(
            checked(
                left.Microseconds +
                right.Microseconds));
    }

    public static SimulationDuration operator -(
        SimulationDuration left,
        SimulationDuration right)
    {
        if (right > left)
        {
            throw new InvalidOperationException(
                "Simulation duration cannot become negative.");
        }

        return new SimulationDuration(
            left.Microseconds -
            right.Microseconds);
    }

    public static bool operator <(
        SimulationDuration left,
        SimulationDuration right)
    {
        return left.Microseconds <
               right.Microseconds;
    }

    public static bool operator >(
        SimulationDuration left,
        SimulationDuration right)
    {
        return left.Microseconds >
               right.Microseconds;
    }

    public static bool operator <=(
        SimulationDuration left,
        SimulationDuration right)
    {
        return left.Microseconds <=
               right.Microseconds;
    }

    public static bool operator >=(
        SimulationDuration left,
        SimulationDuration right)
    {
        return left.Microseconds >=
               right.Microseconds;
    }

    public override string ToString()
    {
        return $"{Microseconds}us";
    }
}