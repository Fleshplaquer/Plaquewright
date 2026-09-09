using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class ScheduledEventKeyTests
{
    [Fact]
    public void EarlierTime_SortsFirst()
    {
        var earlier =
            Create(
                time: 100L,
                wave: 0u,
                SchedulerPhase.Execution,
                sequence: 1UL);

        var later =
            Create(
                time: 200L,
                wave: 0u,
                SchedulerPhase.StateBoundary,
                sequence: 1UL);

        Assert.True(earlier < later);
    }

    [Fact]
    public void EarlierWave_SortsFirstAtSameTime()
    {
        var waveZero =
            Create(
                time: 100L,
                wave: 0u,
                SchedulerPhase.FollowUp,
                sequence: 10UL);

        var waveOne =
            Create(
                time: 100L,
                wave: 1u,
                SchedulerPhase.StateBoundary,
                sequence: 1UL);

        Assert.True(waveZero < waveOne);
    }

    [Fact]
    public void StateBoundary_SortsBeforeExecutionWithinSameWave()
    {
        var boundary =
            Create(
                time: 100L,
                wave: 0u,
                SchedulerPhase.StateBoundary,
                sequence: 2UL);

        var execution =
            Create(
                time: 100L,
                wave: 0u,
                SchedulerPhase.Execution,
                sequence: 1UL);

        Assert.True(boundary < execution);
    }

    [Fact]
    public void Execution_SortsBeforeFollowUpWithinSameWave()
    {
        var execution =
            Create(
                time: 100L,
                wave: 0u,
                SchedulerPhase.Execution,
                sequence: 2UL);

        var followUp =
            Create(
                time: 100L,
                wave: 0u,
                SchedulerPhase.FollowUp,
                sequence: 1UL);

        Assert.True(execution < followUp);
    }

    [Fact]
    public void Sequence_IsFinalTieBreaker()
    {
        var first =
            Create(
                time: 100L,
                wave: 0u,
                SchedulerPhase.Execution,
                sequence: 1UL);

        var second =
            Create(
                time: 100L,
                wave: 0u,
                SchedulerPhase.Execution,
                sequence: 2UL);

        Assert.True(first < second);
    }

    [Fact]
    public void SameComponents_ProduceEqualKeys()
    {
        var first =
            Create(
                100L,
                2u,
                SchedulerPhase.FollowUp,
                42UL);

        var second =
            Create(
                100L,
                2u,
                SchedulerPhase.FollowUp,
                42UL);

        Assert.Equal(first, second);
    }

    [Fact]
    public void LaterWaveCannotJumpAheadUsingEarlierPhase()
    {
        var parentWave =
            Create(
                time: 100L,
                wave: 0u,
                SchedulerPhase.FollowUp,
                sequence: 100UL);

        var childWave =
            Create(
                time: 100L,
                wave: 1u,
                SchedulerPhase.StateBoundary,
                sequence: 1UL);

        Assert.True(
            parentWave < childWave);
    }

    [Fact]
    public void InvalidSequence_IsRejected()
    {
        ScheduledSequence invalid = default;

        Assert.Throws<ArgumentException>(
            () =>
                new ScheduledEventKey(
                    SimulationTime.Zero,
                    SchedulerWave.Initial,
                    SchedulerPhase.Execution,
                    invalid));
    }

    [Fact]
    public void InvalidPhase_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ScheduledEventKey(
                    SimulationTime.Zero,
                    SchedulerWave.Initial,
                    (SchedulerPhase)255,
                    new ScheduledSequence(1UL)));
    }

    private static ScheduledEventKey Create(
        long time,
        uint wave,
        SchedulerPhase phase,
        ulong sequence)
    {
        return new ScheduledEventKey(
            new SimulationTime(time),
            new SchedulerWave(wave),
            phase,
            new ScheduledSequence(sequence));
    }
}