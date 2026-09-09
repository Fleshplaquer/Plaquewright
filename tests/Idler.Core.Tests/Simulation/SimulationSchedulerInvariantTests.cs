using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class SimulationSchedulerInvariantTests
{
    [Fact]
    public void ZeroDelayChain_IncrementsExactlyOneWavePerGeneration()
    {
        var scheduler =
            new SimulationScheduler<string>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 100,
                    maxSameTimestampWave: 10));

        var current =
            scheduler.Schedule(
                SimulationTime.Zero,
                SchedulerPhase.Execution,
                "wave-0");

        Assert.Equal(
            0u,
            current.Wave.Value);

        for (uint expectedWave = 1;
             expectedWave <= 10;
             expectedWave++)
        {
            current =
                scheduler.ScheduleFrom(
                    current,
                    SimulationTime.Zero,
                    SchedulerPhase.Execution,
                    $"wave-{expectedWave}");

            Assert.Equal(
                expectedWave,
                current.Wave.Value);
        }
    }

    [Fact]
    public void MaximumRepresentableWave_IsReportedAsBudgetExceeded()
    {
        var scheduler =
            new SimulationScheduler<string>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 100,
                    maxSameTimestampWave: uint.MaxValue));

        var parent =
            new ScheduledEventKey(
                SimulationTime.Zero,
                new SchedulerWave(uint.MaxValue),
                SchedulerPhase.Execution,
                new ScheduledSequence(1UL));

        var exception =
            Assert.Throws<SimulationBudgetExceededException>(
                () =>
                    scheduler.ScheduleFrom(
                        parent,
                        SimulationTime.Zero,
                        SchedulerPhase.Execution,
                        "impossible-child"));

        Assert.Equal(
            SimulationBudgetKind.SameTimestampWave,
            exception.Kind);
    }

    [Fact]
    public void DequeuedKeys_AreStrictlyIncreasing()
    {
        var scheduler =
            new SimulationScheduler<string>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 100,
                    maxSameTimestampWave: 10));

        var parent =
            scheduler.Schedule(
                new SimulationTime(100L),
                SchedulerPhase.Execution,
                "parent");

        scheduler.Schedule(
            new SimulationTime(200L),
            SchedulerPhase.Execution,
            "later");

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.StateBoundary,
            "boundary");

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.FollowUp,
            "follow-up");

        scheduler.ScheduleFrom(
            parent,
            new SimulationTime(100L),
            SchedulerPhase.StateBoundary,
            "wave-one");

        ScheduledEventKey? previous = null;

        while (scheduler.TryDequeue(
                   out var current))
        {
            if (previous is not null)
            {
                Assert.True(
                    previous.Value <
                    current.Key);
            }

            previous =
                current.Key;
        }
    }
}