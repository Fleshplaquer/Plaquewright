using Plaquewright.Core.Composition;
using Plaquewright.Core.Hosting;
using Plaquewright.Core.Simulation;
using Plaquewright.Core.Stats;

namespace Plaquewright.Core.Tests.Stats;

public sealed class TimedModifierContinuationSnapshotTests
{
    [Fact]
    public void PendingExpiration_RestorePreservesBoundaryAndDerivedQuery()
    {
        var originalState =
            new TimedModifierStateSet();

        var originalTrace =
            new List<TraceObservation>();

        var originalQueries =
            new List<QueryObservation>();

        var originalExpirations =
            new List<ExpirationObservation>();

        var originalSession =
            CreateSession(
                originalState,
                originalTrace,
                originalQueries,
                originalExpirations);

        originalSession.ScheduleExternalInput(
            new SimulationTime(10L),
            new ApplyModifierInput(
                25d,
                new SimulationDuration(100L)));

        originalSession.ScheduleExternalInput(
            new SimulationTime(50L),
            new ObserveResistanceInput());

        originalSession.ScheduleExternalInput(
            new SimulationTime(110L),
            new ObserveResistanceInput());

        //
        // Apply at t=10. This creates the pending
        // StateBoundary expiration at t=110.
        //
        var paused =
            originalSession.RunNext();

        Assert.Equal(
            SimulationRunStatus.InProgress,
            paused.Status);

        Assert.Equal(
            1UL,
            originalState.Revision);

        Assert.Equal(
            125d,
            TimedModifierValueQuery.Evaluate(
                100d,
                originalState));

        var snapshot =
            Capture(
                originalState,
                originalSession);

        var restoredState =
            TimedModifierStateSet.Restore(
                snapshot.State);

        var restoredTrace =
            new List<TraceObservation>();

        var restoredQueries =
            new List<QueryObservation>();

        var restoredExpirations =
            new List<ExpirationObservation>();

        var restoredSession =
            RestoreSession(
                restoredState,
                snapshot.Runner,
                restoredTrace,
                restoredQueries,
                restoredExpirations);

        Assert.NotSame(
            originalState,
            restoredState);

        Assert.Equal(
            originalSession.CurrentTime,
            restoredSession.CurrentTime);

        Assert.Equal(
            originalSession.ProcessedEvents,
            restoredSession.ProcessedEvents);

        //
        // Ignore the already executed pre-snapshot Apply
        // when comparing continuation traces.
        //
        originalTrace.Clear();

        var originalResult =
            originalSession.RunToCompletion();

        //
        // A has now advanced completely before B.
        // Fresh composition/state bindings in B are required.
        //
        Assert.Equal(
            0,
            originalState.ActiveCount);

        Assert.Equal(
            2UL,
            originalState.Revision);

        Assert.Equal(
            1,
            restoredState.ActiveCount);

        Assert.Equal(
            1UL,
            restoredState.Revision);

        var restoredResult =
            restoredSession.RunToCompletion();

        Assert.Equal(
            originalResult,
            restoredResult);

        Assert.Equal(
            SimulationRunStatus.Completed,
            restoredResult.Status);

        Assert.Equal(
            new SimulationTime(110L),
            restoredResult.CurrentTime);

        Assert.Equal(
            4UL,
            restoredResult.ProcessedEvents);

        Assert.Equal(
            originalTrace,
            restoredTrace);

        Assert.Equal(
            originalQueries,
            restoredQueries);

        Assert.Equal(
            originalExpirations,
            restoredExpirations);

        Assert.Equal(
            new[]
            {
                nameof(ObserveResistanceInput),
                nameof(ExpireModifierAction),
                nameof(ObserveResistanceInput)
            },
            originalTrace
                .Select(entry => entry.Kind)
                .ToArray());

        Assert.Equal(
            new[]
            {
                125d,
                100d
            },
            originalQueries
                .Select(query => query.Value)
                .ToArray());

        Assert.Equal(
            new ulong[]
            {
                1UL,
                2UL
            },
            originalQueries
                .Select(query => query.Revision)
                .ToArray());

        Assert.Single(
            originalExpirations);

        Assert.True(
            originalExpirations[0].DidExpire);

        Assert.Equal(
            2UL,
            originalExpirations[0].RevisionAfter);

        Assert.Equal(
            0,
            originalState.ActiveCount);

        Assert.Equal(
            originalState.ActiveCount,
            restoredState.ActiveCount);

        Assert.Equal(
            2UL,
            restoredState.Revision);

        Assert.Equal(
            originalState.Revision,
            restoredState.Revision);
    }

    [Fact]
    public void RefreshedModifier_RestorePreservesStaleAndCurrentExpirations()
    {
        var originalState =
            new TimedModifierStateSet();

        var originalTrace =
            new List<TraceObservation>();

        var originalQueries =
            new List<QueryObservation>();

        var originalExpirations =
            new List<ExpirationObservation>();

        var originalSession =
            CreateSession(
                originalState,
                originalTrace,
                originalQueries,
                originalExpirations);

        //
        // Generation 1 -> expires at 110.
        //
        originalSession.ScheduleExternalInput(
            new SimulationTime(10L),
            new ApplyModifierInput(
                25d,
                new SimulationDuration(100L)));

        //
        // Refresh at t=50 creates generation 2 -> expires at 150.
        // Generation-1 expiration remains physically pending.
        //
        originalSession.ScheduleExternalInput(
            new SimulationTime(50L),
            new ApplyModifierInput(
                40d,
                new SimulationDuration(100L)));

        originalSession.ScheduleExternalInput(
            new SimulationTime(110L),
            new ObserveResistanceInput());

        originalSession.ScheduleExternalInput(
            new SimulationTime(150L),
            new ObserveResistanceInput());

        Assert.Equal(
            SimulationRunStatus.InProgress,
            originalSession.RunNext().Status);

        Assert.Equal(
            SimulationRunStatus.InProgress,
            originalSession.RunNext().Status);

        Assert.Equal(
            2UL,
            originalState.Revision);

        Assert.Equal(
            140d,
            TimedModifierValueQuery.Evaluate(
                100d,
                originalState));

        var snapshot =
            Capture(
                originalState,
                originalSession);

        var restoredState =
            TimedModifierStateSet.Restore(
                snapshot.State);

        var restoredTrace =
            new List<TraceObservation>();

        var restoredQueries =
            new List<QueryObservation>();

        var restoredExpirations =
            new List<ExpirationObservation>();

        var restoredSession =
            RestoreSession(
                restoredState,
                snapshot.Runner,
                restoredTrace,
                restoredQueries,
                restoredExpirations);

        originalTrace.Clear();

        var originalResult =
            originalSession.RunToCompletion();

        Assert.Equal(
            0,
            originalState.ActiveCount);

        Assert.Equal(
            3UL,
            originalState.Revision);

        //
        // Before B runs it still represents the exact
        // snapshot boundary.
        //
        Assert.Equal(
            1,
            restoredState.ActiveCount);

        Assert.Equal(
            2UL,
            restoredState.Revision);

        var restoredResult =
            restoredSession.RunToCompletion();

        Assert.Equal(
            originalResult,
            restoredResult);

        Assert.Equal(
            originalTrace,
            restoredTrace);

        Assert.Equal(
            originalQueries,
            restoredQueries);

        Assert.Equal(
            originalExpirations,
            restoredExpirations);

        Assert.Equal(
            new[]
            {
                nameof(ExpireModifierAction),
                nameof(ObserveResistanceInput),
                nameof(ExpireModifierAction),
                nameof(ObserveResistanceInput)
            },
            originalTrace
                .Select(entry => entry.Kind)
                .ToArray());

        Assert.Equal(
            2,
            originalExpirations.Count);

        //
        // Old generation at t=110 is expected stale work.
        // It must not mutate state or invalidate the cache.
        //
        Assert.False(
            originalExpirations[0].DidExpire);

        Assert.Equal(
            2UL,
            originalExpirations[0].RevisionAfter);

        Assert.Equal(
            140d,
            originalQueries[0].Value);

        Assert.Equal(
            2UL,
            originalQueries[0].Revision);

        //
        // Generation 2 expires at its real boundary t=150.
        //
        Assert.True(
            originalExpirations[1].DidExpire);

        Assert.Equal(
            3UL,
            originalExpirations[1].RevisionAfter);

        Assert.Equal(
            100d,
            originalQueries[1].Value);

        Assert.Equal(
            3UL,
            originalQueries[1].Revision);

        Assert.Equal(
            0,
            restoredState.ActiveCount);

        Assert.Equal(
            originalState.Revision,
            restoredState.Revision);
    }

    private static SimulationSession<TestWorkItem>
        CreateSession(
            TimedModifierStateSet state,
            List<TraceObservation> trace,
            List<QueryObservation> queries,
            List<ExpirationObservation> expirations)
    {
        var composition =
            CreateComposition(
                state,
                trace,
                queries,
                expirations);

        return new SimulationSession<TestWorkItem>(
            composition,
            new SimulationSchedulerLimits(
                maxQueueSize: 20,
                maxSameTimestampWave: 10),
            new SimulationRunnerLimits(
                maxProcessedEvents: 20UL));
    }

    private static SimulationSession<TestWorkItem>
        RestoreSession(
            TimedModifierStateSet state,
            SimulationRunnerSnapshot<TestWorkItemSnapshot>
                runnerSnapshot,
            List<TraceObservation> trace,
            List<QueryObservation> queries,
            List<ExpirationObservation> expirations)
    {
        return SimulationSession<TestWorkItem>
            .Restore<TestWorkItemSnapshot>(
                CreateComposition(
                    state,
                    trace,
                    queries,
                    expirations),
                runnerSnapshot,
                RestoreWorkItem);
    }

    private static SimulationComposition<TestWorkItem>
        CreateComposition(
            TimedModifierStateSet state,
            List<TraceObservation> trace,
            List<QueryObservation> queries,
            List<ExpirationObservation> expirations)
    {
        var cache =
            new TimedModifierValueQueryCache();

        var key =
            TimedModifierKey.Parse(
                "status.resistance_boost");

        var builder =
            new SimulationCompositionBuilder<TestWorkItem>();

        builder.AddModule(
            "TimedModifierContinuation",
            module =>
            {
                module.Handle<ApplyModifierInput>(
                    (input, context) =>
                    {
                        trace.Add(
                            new TraceObservation(
                                context.Key,
                                nameof(ApplyModifierInput)));

                        var expiration =
                            state.ApplyOrRefresh(
                                key,
                                ModifierKind.Flat,
                                input.Amount,
                                context.CurrentTime,
                                input.Duration);

                        context.Schedule(
                            expiration.ExpiresAt,
                            SchedulerPhase.StateBoundary,
                            new ExpireModifierAction(
                                expiration));
                    });

                module.Handle<ExpireModifierAction>(
                    (action, context) =>
                    {
                        var didExpire =
                            state.Expire(
                                action.Expiration,
                                context.CurrentTime);

                        trace.Add(
                            new TraceObservation(
                                context.Key,
                                nameof(ExpireModifierAction)));

                        expirations.Add(
                            new ExpirationObservation(
                                context.Key,
                                action.Expiration.Generation,
                                didExpire,
                                state.Revision));
                    });

                module.Handle<ObserveResistanceInput>(
                    (_, context) =>
                    {
                        var reference =
                            TimedModifierValueQuery.Evaluate(
                                100d,
                                state);

                        var cached =
                            cache.Evaluate(
                                100d,
                                state);

                        Assert.Equal(
                            reference,
                            cached);

                        trace.Add(
                            new TraceObservation(
                                context.Key,
                                nameof(ObserveResistanceInput)));

                        queries.Add(
                            new QueryObservation(
                                context.Key,
                                cached,
                                state.Revision));
                    });
            });

        return builder.Build();
    }

    private static ContinuationSnapshot Capture(
        TimedModifierStateSet state,
        SimulationSession<TestWorkItem> session)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        ArgumentNullException.ThrowIfNull(
            session);

        //
        // As in the S05 continuation envelope, capture the
        // execution side first so quiescence checks happen
        // before domain-state capture is returned.
        //
        var runner =
            session.CaptureRunnerSnapshot(
                CaptureWorkItem);

        var stateSnapshot =
            state.CaptureSnapshot();

        return new ContinuationSnapshot(
            stateSnapshot,
            runner);
    }

    private static TestWorkItemSnapshot CaptureWorkItem(
        TestWorkItem workItem)
    {
        ArgumentNullException.ThrowIfNull(
            workItem);

        return workItem switch
        {
            ApplyModifierInput apply =>
                new ApplyModifierInputSnapshot(
                    apply.Amount,
                    apply.Duration),

            ExpireModifierAction expire =>
                new ExpireModifierActionSnapshot(
                    expire.Expiration),

            ObserveResistanceInput =>
                new ObserveResistanceInputSnapshot(),

            _ =>
                throw new NotSupportedException(
                    $"Unsupported test work item type '{workItem.GetType().FullName}'.")
        };
    }

    private static TestWorkItem RestoreWorkItem(
        TestWorkItemSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(
            snapshot);

        return snapshot switch
        {
            ApplyModifierInputSnapshot apply =>
                new ApplyModifierInput(
                    apply.Amount,
                    apply.Duration),

            ExpireModifierActionSnapshot expire =>
                new ExpireModifierAction(
                    expire.Expiration),

            ObserveResistanceInputSnapshot =>
                new ObserveResistanceInput(),

            _ =>
                throw new NotSupportedException(
                    $"Unsupported test work item snapshot type '{snapshot.GetType().FullName}'.")
        };
    }

    private sealed record ContinuationSnapshot(
        TimedModifierStateSetSnapshot State,
        SimulationRunnerSnapshot<TestWorkItemSnapshot>
            Runner);

    private abstract record TestWorkItem
        : ISimulationWorkItem;

    private sealed record ApplyModifierInput(
        double Amount,
        SimulationDuration Duration)
        : TestWorkItem;

    private sealed record ExpireModifierAction(
        TimedModifierExpiration Expiration)
        : TestWorkItem;

    private sealed record ObserveResistanceInput
        : TestWorkItem;

    private abstract record TestWorkItemSnapshot;

    private sealed record ApplyModifierInputSnapshot(
        double Amount,
        SimulationDuration Duration)
        : TestWorkItemSnapshot;

    private sealed record ExpireModifierActionSnapshot(
        TimedModifierExpiration Expiration)
        : TestWorkItemSnapshot;

    private sealed record ObserveResistanceInputSnapshot
        : TestWorkItemSnapshot;

    private readonly record struct TraceObservation(
        ScheduledEventKey Key,
        string Kind);

    private readonly record struct QueryObservation(
        ScheduledEventKey Key,
        double Value,
        ulong Revision);

    private readonly record struct ExpirationObservation(
        ScheduledEventKey Key,
        ulong Generation,
        bool DidExpire,
        ulong RevisionAfter);
}