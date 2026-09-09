namespace Idler.Core.Simulation;

public readonly record struct ScheduledEventKey
    : IComparable<ScheduledEventKey>
{
    public SimulationTime Time { get; }

    public SchedulerWave Wave { get; }

    public SchedulerPhase Phase { get; }

    public ScheduledSequence Sequence { get; }

    public ScheduledEventKey(
        SimulationTime time,
        SchedulerWave wave,
        SchedulerPhase phase,
        ScheduledSequence sequence)
    {
        if (!sequence.IsValid)
        {
            throw new ArgumentException(
                "Scheduled sequence must be valid.",
                nameof(sequence));
        }

        if (!Enum.IsDefined(phase))
        {
            throw new ArgumentOutOfRangeException(
                nameof(phase),
                phase,
                "Unknown scheduler phase.");
        }

        Time = time;
        Wave = wave;
        Phase = phase;
        Sequence = sequence;
    }

    public int CompareTo(ScheduledEventKey other)
    {
        var timeComparison =
            Time.CompareTo(other.Time);

        if (timeComparison != 0)
        {
            return timeComparison;
        }

        var waveComparison =
            Wave.CompareTo(other.Wave);

        if (waveComparison != 0)
        {
            return waveComparison;
        }

        var phaseComparison =
            Phase.CompareTo(other.Phase);

        if (phaseComparison != 0)
        {
            return phaseComparison;
        }

        return Sequence.CompareTo(
            other.Sequence);
    }

    public static bool operator <(
        ScheduledEventKey left,
        ScheduledEventKey right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(
        ScheduledEventKey left,
        ScheduledEventKey right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(
        ScheduledEventKey left,
        ScheduledEventKey right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(
        ScheduledEventKey left,
        ScheduledEventKey right)
    {
        return left.CompareTo(right) >= 0;
    }
}