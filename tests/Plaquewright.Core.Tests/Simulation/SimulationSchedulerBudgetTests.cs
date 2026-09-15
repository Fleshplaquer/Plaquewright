using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationSchedulerBudgetTests
{
    [Fact]
    public void QueueSizeLimit_AllowsExactlyConfiguredAmount()
    {
        var scheduler =
            CreateScheduler(
                maxQueueSize: 2,
                maxWave: 10);

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "first");

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "second");

        Assert.Equal(
            2,
            scheduler.Count);
    }

    [Fact]
    public void QueueSizeLimit_RejectsAdditionalEvent()
    {
        var scheduler =
            CreateScheduler(
                maxQueueSize: 2,
                maxWave: 10);

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "first");

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "second");

        var exception =
            Assert.Throws<SimulationBudgetExceededException>(
                () =>
                    scheduler.Schedule(
                        SimulationTime.Zero,
                        SchedulerPhase.Execution,
                        "third"));

        Assert.Equal(
            SimulationBudgetKind.QueueSize,
            exception.Kind);

        Assert.Equal(
            2,
            scheduler.Count);
    }

    [Fact]
    public void Dequeue_FreesQueueCapacity()
    {
        var scheduler =
            CreateScheduler(
                maxQueueSize: 1,
                maxWave: 10);

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "first");

        Assert.True(
            scheduler.TryDequeue(out _));

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "second");

        Assert.Equal(
            1,
            scheduler.Count);
    }

    [Fact]
    public void SameTimestampWaveLimit_AllowsConfiguredWave()
    {
        var scheduler =
            CreateScheduler(
                maxQueueSize: 100,
                maxWave: 2);

        var waveZero =
            scheduler.Schedule(
                SimulationTime.Zero,
                SchedulerPhase.Execution,
                "wave-zero");

        var waveOne =
            scheduler.ScheduleFrom(
                waveZero,
                SimulationTime.Zero,
                SchedulerPhase.Execution,
                "wave-one");

        var waveTwo =
            scheduler.ScheduleFrom(
                waveOne,
                SimulationTime.Zero,
                SchedulerPhase.Execution,
                "wave-two");

        Assert.Equal(
            2u,
            waveTwo.Wave.Value);
    }

    [Fact]
    public void SameTimestampWaveLimit_RejectsNextWave()
    {
        var scheduler =
            CreateScheduler(
                maxQueueSize: 100,
                maxWave: 2);

        var waveZero =
            scheduler.Schedule(
                SimulationTime.Zero,
                SchedulerPhase.Execution,
                "wave-zero");

        var waveOne =
            scheduler.ScheduleFrom(
                waveZero,
                SimulationTime.Zero,
                SchedulerPhase.Execution,
                "wave-one");

        var waveTwo =
            scheduler.ScheduleFrom(
                waveOne,
                SimulationTime.Zero,
                SchedulerPhase.Execution,
                "wave-two");

        var exception =
            Assert.Throws<SimulationBudgetExceededException>(
                () =>
                    scheduler.ScheduleFrom(
                        waveTwo,
                        SimulationTime.Zero,
                        SchedulerPhase.Execution,
                        "wave-three"));

        Assert.Equal(
            SimulationBudgetKind.SameTimestampWave,
            exception.Kind);
    }

    [Fact]
    public void FutureEvent_ResetsWaveAndDoesNotViolateSameTimestampLimit()
    {
        var scheduler =
            CreateScheduler(
                maxQueueSize: 100,
                maxWave: 1);

        var waveZero =
            scheduler.Schedule(
                SimulationTime.Zero,
                SchedulerPhase.Execution,
                "start");

        var waveOne =
            scheduler.ScheduleFrom(
                waveZero,
                SimulationTime.Zero,
                SchedulerPhase.Execution,
                "zero-delay-child");

        var future =
            scheduler.ScheduleFrom(
                waveOne,
                new SimulationTime(1L),
                SchedulerPhase.Execution,
                "future");

        Assert.Equal(
            SchedulerWave.Initial,
            future.Wave);
    }

    [Fact]
    public void WaveZeroOnlyConfiguration_RejectsZeroDelayChildren()
    {
        var scheduler =
            CreateScheduler(
                maxQueueSize: 100,
                maxWave: 0);

        var parent =
            scheduler.Schedule(
                SimulationTime.Zero,
                SchedulerPhase.Execution,
                "parent");

        var exception =
            Assert.Throws<SimulationBudgetExceededException>(
                () =>
                    scheduler.ScheduleFrom(
                        parent,
                        SimulationTime.Zero,
                        SchedulerPhase.Execution,
                        "child"));

        Assert.Equal(
            SimulationBudgetKind.SameTimestampWave,
            exception.Kind);
    }

    private static SimulationScheduler<string>
        CreateScheduler(
            int maxQueueSize,
            uint maxWave)
    {
        return new SimulationScheduler<string>(
            new SimulationSchedulerLimits(
                maxQueueSize,
                maxWave));
    }
}