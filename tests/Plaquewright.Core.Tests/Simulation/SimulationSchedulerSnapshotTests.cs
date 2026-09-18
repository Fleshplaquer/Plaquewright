using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationSchedulerSnapshotTests
{
    [Fact]
    public void SnapshotRestore_PreservesPendingEventsKeysSequenceAndLimits()
    {
        var scheduler =
            new SimulationScheduler<string>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 3,
                    maxSameTimestampWave: 4));

        var firstKey =
            scheduler.Schedule(
                new SimulationTime(10L),
                SchedulerPhase.Execution,
                "first");

        var secondKey =
            scheduler.Schedule(
                new SimulationTime(20L),
                SchedulerPhase.FollowUp,
                "second");

        var snapshot =
            scheduler.CaptureSnapshot(
                static payload => payload);

        Assert.Equal(
            3,
            snapshot.MaxQueueSize);

        Assert.Equal(
            4u,
            snapshot.MaxSameTimestampWave);

        Assert.Equal(
            3UL,
            snapshot.NextSequenceValue);

        var restored =
            SimulationScheduler<string>.Restore(
                snapshot,
                static payload => payload);

        Assert.Equal(
            2,
            restored.Count);

        Assert.True(
            restored.TryDequeue(
                out var first));

        Assert.Equal(
            firstKey,
            first.Key);

        Assert.Equal(
            "first",
            first.Payload);

        Assert.True(
            restored.TryDequeue(
                out var second));

        Assert.Equal(
            secondKey,
            second.Key);

        Assert.Equal(
            "second",
            second.Payload);

        var thirdKey =
            restored.Schedule(
                new SimulationTime(30L),
                SchedulerPhase.Execution,
                "third");

        Assert.Equal(
            3UL,
            thirdKey.Sequence.Value);
    }
    [Fact]
    public void SnapshotCapture_DuringActiveEvent_IsRejected()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "parent");

        Assert.True(
            scheduler.TryDequeue(
                out var parent));

        scheduler.BeginEventExecution(
            parent.Key);

        try
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                    scheduler.CaptureSnapshot(
                        static payload => payload));
        }
        finally
        {
            scheduler.EndEventExecution(
                parent.Key);
        }
    }
    [Fact]
    public void SnapshotCapture_WithOutstandingPreparedFollowUp_IsRejected()
    {
        var scheduler =
            new SimulationScheduler<string>();

        scheduler.Schedule(
            SimulationTime.Zero,
            SchedulerPhase.Execution,
            "parent");

        Assert.True(
            scheduler.TryDequeue(
                out var parent));

        scheduler.BeginEventExecution(
            parent.Key);

        var prepared =
            scheduler.PrepareFollowUpFromActiveEvent(
                parent.Key,
                parent.Key.Time);

        scheduler.EndEventExecution(
            parent.Key);

        try
        {
            Assert.Throws<InvalidOperationException>(
                () =>
                    scheduler.CaptureSnapshot(
                        static payload => payload));
        }
        finally
        {
            prepared.Dispose();
        }

        //
        // Once the reservation is released, the
        // scheduler is quiescent again.
        //
        var snapshot =
            scheduler.CaptureSnapshot(
                static payload => payload);

        Assert.Empty(
            snapshot.PendingEvents);
    }
}