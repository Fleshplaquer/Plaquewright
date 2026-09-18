using Plaquewright.Core.Composition;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Hosting;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Hosting;

public sealed class SimulationRuntimeSessionSnapshotTests
{
    [Fact]
    public void SnapshotRestore_RebuildsSessionAgainstRestoredRuntime()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
                Array.Empty<ResourceDefinition>());

        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var source =
            runtime.CreateEntity(
                []);

        var beforeSnapshot =
            runtime.CreateExecutionContext(
                source.Id,
                SimulationTime.Zero);

        Assert.Equal(
            new ExecutionId(1UL),
            beforeSnapshot.Id);

        var originalTrace =
            new List<ExecutionObservation>();

        var originalComposition =
            CreateComposition(
                runtime,
                originalTrace);

        var originalSession =
            new SimulationSession<CreateExecutionAction>(
                originalComposition,
                new SimulationSchedulerLimits(
                    maxQueueSize: 10,
                    maxSameTimestampWave: 10),
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        var firstKey =
            originalSession.ScheduleExternalInput(
                new SimulationTime(100L),
                new CreateExecutionAction(
                    source.Id));

        var secondKey =
            originalSession.ScheduleExternalInput(
                new SimulationTime(200L),
                new CreateExecutionAction(
                    source.Id));

        var paused =
            originalSession.RunNext();

        Assert.Equal(
            SimulationRunStatus.InProgress,
            paused.Status);

        Assert.Equal(
            new SimulationTime(100L),
            originalSession.CurrentTime);

        Assert.Equal(
            1UL,
            originalSession.ProcessedEvents);

        Assert.Single(
            originalTrace);

        Assert.Equal(
            firstKey,
            originalTrace[0].Key);

        Assert.Equal(
            new ExecutionId(2UL),
            originalTrace[0].ExecutionId);

        var snapshot =
            SimulationRuntimeSessionSnapshot<
                CreateExecutionActionSnapshot>.Capture(
                    runtime,
                    originalSession,
                    CaptureWorkItem);

        Assert.Equal(
            runtime.RootSeed,
            snapshot.Runtime.RootSeed);

        Assert.Equal(
            originalSession.CurrentTime,
            snapshot.Runner.CurrentTime);

        Assert.Equal(
            originalSession.ProcessedEvents,
            snapshot.Runner.ProcessedEvents);

        Assert.Single(
            snapshot.Runner.Scheduler.PendingEvents);

        Assert.Equal(
            secondKey,
            snapshot.Runner.Scheduler
                .PendingEvents[0].Key);

        var restoredRuntime =
            SimulationRuntimeState.Restore(
                snapshot.Runtime,
                registry);

        Assert.NotSame(
            runtime,
            restoredRuntime);

        var restoredTrace =
            new List<ExecutionObservation>();

        //
        // Composition is deliberately rebuilt against
        // Runtime B. It is not part of the snapshot.
        //
        var restoredComposition =
            CreateComposition(
                restoredRuntime,
                restoredTrace);

        var restoredSession =
            SimulationSession<CreateExecutionAction>
                .Restore(
                    restoredComposition,
                    snapshot.Runner,
                    RestoreWorkItem);

        Assert.Equal(
            originalSession.CurrentTime,
            restoredSession.CurrentTime);

        Assert.Equal(
            originalSession.ProcessedEvents,
            restoredSession.ProcessedEvents);

        //
        // D-04 survives through the higher session restore:
        // timestamp 100 has already begun.
        //
        Assert.Throws<InvalidOperationException>(
            () =>
                restoredSession.ScheduleExternalInput(
                    new SimulationTime(100L),
                    new CreateExecutionAction(
                        source.Id)));

        //
        // Compare continuation only.
        //
        originalTrace.Clear();

        var originalResult =
            originalSession.RunToCompletion();

        Assert.Equal(
            SimulationRunStatus.Completed,
            originalResult.Status);

        Assert.Single(
            originalTrace);

        Assert.Equal(
            secondKey,
            originalTrace[0].Key);

        Assert.Equal(
            new ExecutionId(3UL),
            originalTrace[0].ExecutionId);

        //
        // Runtime A has already completed before B starts.
        // A stale Composition/handler binding therefore
        // becomes visible immediately.
        //
        var restoredResult =
            restoredSession.RunToCompletion();

        Assert.Equal(
            SimulationRunStatus.Completed,
            restoredResult.Status);

        Assert.Equal(
            originalResult.CurrentTime,
            restoredResult.CurrentTime);

        Assert.Equal(
            originalResult.ProcessedEvents,
            restoredResult.ProcessedEvents);

        Assert.Equal(
            originalResult.PendingEvents,
            restoredResult.PendingEvents);

        Assert.Equal(
            originalTrace,
            restoredTrace);

        Assert.Equal(
            new SimulationTime(200L),
            restoredSession.CurrentTime);

        Assert.Equal(
            2UL,
            restoredSession.ProcessedEvents);

        //
        // Both allocator branches continue independently
        // from the same captured position.
        //
        var originalNext =
            runtime.CreateExecutionContext(
                source.Id,
                new SimulationTime(300L));

        var restoredNext =
            restoredRuntime.CreateExecutionContext(
                source.Id,
                new SimulationTime(300L));

        Assert.Equal(
            new ExecutionId(4UL),
            originalNext.Id);

        Assert.Equal(
            originalNext.Id,
            restoredNext.Id);
    }

    [Fact]
    public void Capture_NullBoundariesAreRejected()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
                Array.Empty<ResourceDefinition>());

        var runtime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var composition =
            CreateComposition(
                runtime,
                new List<ExecutionObservation>());

        var session =
            new SimulationSession<CreateExecutionAction>(
                composition,
                new SimulationSchedulerLimits(
                    maxQueueSize: 10,
                    maxSameTimestampWave: 10),
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        Assert.Throws<ArgumentNullException>(
    () =>
        SimulationRuntimeSessionSnapshot<
            CreateExecutionActionSnapshot>.Capture<
                CreateExecutionAction>(
                    null!,
                    session,
                    CaptureWorkItem));

        Assert.Throws<ArgumentNullException>(
            () =>
                SimulationRuntimeSessionSnapshot<
                    CreateExecutionActionSnapshot>.Capture<
                        CreateExecutionAction>(
                            runtime,
                            null!,
                            CaptureWorkItem));

        Assert.Throws<ArgumentNullException>(
            () =>
                SimulationRuntimeSessionSnapshot<
                    CreateExecutionActionSnapshot>.Capture<
                        CreateExecutionAction>(
                            runtime,
                            session,
                            null!));
    }

    private static SimulationComposition<
        CreateExecutionAction> CreateComposition(
            SimulationRuntimeState runtime,
            List<ExecutionObservation> trace)
    {
        var builder =
            new SimulationCompositionBuilder<
                CreateExecutionAction>();

        builder.AddModule(
            "Execution",
            module =>
            {
                module.Handle<CreateExecutionAction>(
                    (action, context) =>
                    {
                        var execution =
                            runtime.CreateExecutionContext(
                                action.SourceEntityId,
                                context.CurrentTime);

                        trace.Add(
                            new ExecutionObservation(
                                context.Key,
                                execution.Id));
                    });
            });

        return builder.Build();
    }

    private static CreateExecutionActionSnapshot
        CaptureWorkItem(
            CreateExecutionAction action)
    {
        ArgumentNullException.ThrowIfNull(
            action);

        return new CreateExecutionActionSnapshot(
            action.SourceEntityId);
    }

    private static CreateExecutionAction
        RestoreWorkItem(
            CreateExecutionActionSnapshot snapshot)
    {
        return new CreateExecutionAction(
            snapshot.SourceEntityId);
    }

    private sealed record CreateExecutionAction(
        EntityId SourceEntityId)
        : ISimulationWorkItem;

    private readonly record struct
        CreateExecutionActionSnapshot(
            EntityId SourceEntityId);

    private readonly record struct
        ExecutionObservation(
            ScheduledEventKey Key,
            ExecutionId ExecutionId);
}