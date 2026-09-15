using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationSchedulerTests
{
    [Fact]
    public void NewScheduler_IsEmpty()
    {
        var scheduler =
            new SimulationScheduler<string>();

        Assert.True(scheduler.IsEmpty);
        Assert.Equal(0, scheduler.Count);
    }

    [Fact]
    public void Schedule_AddsEvent()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.Execution,
            "event");

        Assert.False(scheduler.IsEmpty);
        Assert.Equal(1, scheduler.Count);
    }

    [Fact]
    public void Dequeue_ReturnsEarliestTimeFirst()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            new SimulationTime(300L),
            SchedulerPhase.Execution,
            "third");

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.Execution,
            "first");

        scheduler.Schedule(
            new SimulationTime(200L),
            SchedulerPhase.Execution,
            "second");

        Assert.Equal(
            "first",
            Dequeue(scheduler).Payload);

        Assert.Equal(
            "second",
            Dequeue(scheduler).Payload);

        Assert.Equal(
            "third",
            Dequeue(scheduler).Payload);
    }

    [Fact]
    public void SameTimeAndWave_UsesSemanticPhase()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.FollowUp,
            "follow-up");

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.Execution,
            "execution");

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.StateBoundary,
            "boundary");

        Assert.Equal(
            "boundary",
            Dequeue(scheduler).Payload);

        Assert.Equal(
            "execution",
            Dequeue(scheduler).Payload);

        Assert.Equal(
            "follow-up",
            Dequeue(scheduler).Payload);
    }

    [Fact]
    public void SameTimeWaveAndPhase_UsesSequence()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.Execution,
            "first");

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.Execution,
            "second");

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.Execution,
            "third");

        Assert.Equal(
            "first",
            Dequeue(scheduler).Payload);

        Assert.Equal(
            "second",
            Dequeue(scheduler).Payload);

        Assert.Equal(
            "third",
            Dequeue(scheduler).Payload);
    }

    [Fact]
    public void ZeroDelayChild_IsScheduledInNextWave()
    {
        var scheduler =
            new SimulationScheduler<string>();

        var parentKey =
            scheduler.Schedule(
                new SimulationTime(100L),
                SchedulerPhase.Execution,
                "parent");

        var childKey =
            scheduler.ScheduleFrom(
                parentKey,
                new SimulationTime(100L),
                SchedulerPhase.StateBoundary,
                "child");

        Assert.Equal(
            parentKey.Wave.Next(),
            childKey.Wave);

        Assert.Equal(
            parentKey.Time,
            childKey.Time);
    }

    [Fact]
    public void ZeroDelayChild_CannotJumpAheadOfRemainingParentWave()
    {
        var scheduler =
            new SimulationScheduler<string>();

        var parentKey =
            scheduler.Schedule(
                new SimulationTime(100L),
                SchedulerPhase.Execution,
                "parent");

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.FollowUp,
            "remaining-wave-zero");

        scheduler.ScheduleFrom(
            parentKey,
            new SimulationTime(100L),
            SchedulerPhase.StateBoundary,
            "child-wave-one");

        Assert.Equal(
            "parent",
            Dequeue(scheduler).Payload);

        Assert.Equal(
            "remaining-wave-zero",
            Dequeue(scheduler).Payload);

        Assert.Equal(
            "child-wave-one",
            Dequeue(scheduler).Payload);
    }

    [Fact]
    public void FutureChild_ResetsToInitialWave()
    {
        var scheduler =
            new SimulationScheduler<string>();

        var parentKey =
            scheduler.Schedule(
                new SimulationTime(100L),
                SchedulerPhase.Execution,
                "parent");

        var childKey =
            scheduler.ScheduleFrom(
                parentKey,
                new SimulationTime(200L),
                SchedulerPhase.Execution,
                "future");

        Assert.Equal(
            SchedulerWave.Initial,
            childKey.Wave);
    }

    [Fact]
    public void ScheduleFrom_CannotScheduleBeforeParent()
    {
        var scheduler =
            new SimulationScheduler<string>();

        var parentKey =
            scheduler.Schedule(
                new SimulationTime(100L),
                SchedulerPhase.Execution,
                "parent");

        Assert.Throws<InvalidOperationException>(
            () =>
                scheduler.ScheduleFrom(
                    parentKey,
                    new SimulationTime(99L),
                    SchedulerPhase.Execution,
                    "invalid"));
    }

    [Fact]
    public void TryPeek_DoesNotRemoveEvent()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.Execution,
            "event");

        Assert.True(
            scheduler.TryPeek(
                out var result));

        Assert.Equal(
            "event",
            result.Payload);

        Assert.Equal(
            1,
            scheduler.Count);
    }

    [Fact]
    public void TryDequeue_RemovesEvent()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            new SimulationTime(100L),
            SchedulerPhase.Execution,
            "event");

        Assert.True(
            scheduler.TryDequeue(
                out var result));

        Assert.Equal(
            "event",
            result.Payload);

        Assert.True(
            scheduler.IsEmpty);
    }

    [Fact]
    public void EmptyScheduler_TryPeekReturnsFalse()
    {
        var scheduler =
            new SimulationScheduler<string>();

        Assert.False(
            scheduler.TryPeek(out _));
    }

    [Fact]
    public void EmptyScheduler_TryDequeueReturnsFalse()
    {
        var scheduler =
            new SimulationScheduler<string>();

        Assert.False(
            scheduler.TryDequeue(out _));
    }

    private static ScheduledEvent<string> Dequeue(
        SimulationScheduler<string> scheduler)
    {
        var success =
            scheduler.TryDequeue(
                out var scheduledEvent);

        Assert.True(success);

        return scheduledEvent;
    }
}