namespace Idler.Core.Simulation;

public readonly record struct SimulationTime
    : IComparable<SimulationTime>
{
    public static SimulationTime Zero => new(0L);

    public long Microseconds { get; }

    public SimulationTime(long microseconds)
    {
        if (microseconds < 0L)
        {
            throw new ArgumentOutOfRangeException(
                nameof(microseconds),
                microseconds,
                "Simulation time cannot be negative.");
        }

        Microseconds = microseconds;
    }

    public int CompareTo(
        SimulationTime other)
    {
        return Microseconds.CompareTo(
            other.Microseconds);
    }

    public static SimulationTime operator +(
        SimulationTime time,
        SimulationDuration duration)
    {
        return new SimulationTime(
            checked(
                time.Microseconds +
                duration.Microseconds));
    }

    public static SimulationTime operator -(
        SimulationTime time,
        SimulationDuration duration)
    {
        if (duration.Microseconds >
            time.Microseconds)
        {
            throw new InvalidOperationException(
                "Simulation time cannot become negative.");
        }

        return new SimulationTime(
            time.Microseconds -
            duration.Microseconds);
    }

    public static SimulationDuration operator -(
        SimulationTime later,
        SimulationTime earlier)
    {
        if (earlier > later)
        {
            throw new InvalidOperationException(
                "Cannot produce a negative simulation duration.");
        }

        return new SimulationDuration(
            later.Microseconds -
            earlier.Microseconds);
    }

    public static bool operator <(
        SimulationTime left,
        SimulationTime right)
    {
        return left.Microseconds <
               right.Microseconds;
    }

    public static bool operator >(
        SimulationTime left,
        SimulationTime right)
    {
        return left.Microseconds >
               right.Microseconds;
    }

    public static bool operator <=(
        SimulationTime left,
        SimulationTime right)
    {
        return left.Microseconds <=
               right.Microseconds;
    }

    public static bool operator >=(
        SimulationTime left,
        SimulationTime right)
    {
        return left.Microseconds >=
               right.Microseconds;
    }

    public override string ToString()
    {
        return $"{Microseconds}us";
    }
}