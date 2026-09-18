using Plaquewright.Core.Combat;
using Plaquewright.Core.Composition;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;
using Plaquewright.Core.Hosting;

namespace Plaquewright.Core.Tests.Combat;

public sealed class CombatWorkItemSnapshotCodecTests
{
    [Fact]
    public void MixedPendingWork_RestorePreservesFactsAndIndependentContinuation()
    {
        var (runtime, sourceId, targetId, lifeId) = CreateRuntime();
        var firstAction = CreateDamage(
            runtime, sourceId, targetId, 40d, new SimulationTime(10L), true);
        var secondAction = CreateDamage(
            runtime, sourceId, targetId, 25d, new SimulationTime(20L), false);

        var runner = CreateRunner();
        var firstKey = runner.ScheduleExternalInput(
            new SimulationTime(100L), firstAction);
        var secondKey = runner.ScheduleExternalInput(
            new SimulationTime(200L), secondAction);

        var ledger = new ResourceOperationLedger();
        var trace = new List<(ScheduledEventKey Key, string Kind)>();
        var observations = new List<EventObservation>();
        var composition = CreateComposition(
            runtime, lifeId, ledger, trace, observations);

        var paused = runner.RunNext(composition.Execute);
        var life = runtime.Entities.Get(targetId).Resources.Get(lifeId);

        Assert.Equal(SimulationRunStatus.InProgress, paused.Status);
        Assert.Equal(1UL, paused.ProcessedEvents);
        Assert.Equal(60d, life.Current);
        Assert.Equal(1UL, life.Revision);
        Assert.Equal(1, ledger.Count);
        Assert.Empty(observations);

        // Pause AFTER the first commit, BEFORE its event is dispatched.
        var runtimeSnapshot = runtime.CaptureSnapshot();
        var runnerSnapshot = runner.CaptureSnapshot(
            CombatWorkItemSnapshotCodec.Capture);
        var pending = runnerSnapshot.Scheduler.PendingEvents;

        Assert.Equal(2, pending.Count);
        Assert.IsType<CombatWorkItemSnapshot.CommittedDamage>(pending[0].Payload);
        Assert.IsType<CombatWorkItemSnapshot.ResolvedDamage>(pending[1].Payload);
        Assert.Equal(firstKey.Time, pending[0].Key.Time);
        Assert.Equal(1u, pending[0].Key.Wave.Value);
        Assert.Equal(SchedulerPhase.FollowUp, pending[0].Key.Phase);
        Assert.Equal(3UL, pending[0].Key.Sequence.Value);
        Assert.Equal(secondKey, pending[1].Key);
        Assert.Equal(4UL, runnerSnapshot.Scheduler.NextSequenceValue);

        var restoredRuntime = SimulationRuntimeState.Restore(
            runtimeSnapshot, runtime.ResourceRegistry);
        var restoredRunner = SimulationRunner<ISimulationWorkItem>.Restore(
            runnerSnapshot,
            snapshot => CombatWorkItemSnapshotCodec.Restore(
                snapshot, restoredRuntime));
        var restoredLife = restoredRuntime.Entities.Get(targetId)
            .Resources.Get(lifeId);

        Assert.NotSame(life, restoredLife);
        Assert.Equal(60d, restoredLife.Current);
        Assert.Equal(1UL, restoredLife.Revision);
        Assert.Equal(runner.CurrentTime, restoredRunner.CurrentTime);
        Assert.Equal(runner.ProcessedEvents, restoredRunner.ProcessedEvents);

        // Ledger history before the boundary is NOT restored by this profile.
        // Compare only new bookings; never replay the already committed damage.
        var ledgerPrefixCount = ledger.Count;
        trace.Clear();
        var restoredLedger = new ResourceOperationLedger();
        var restoredTrace = new List<(ScheduledEventKey Key, string Kind)>();
        var restoredObservations = new List<EventObservation>();
        var restoredComposition = CreateComposition(
            restoredRuntime, lifeId, restoredLedger,
            restoredTrace, restoredObservations);

        var result = runner.RunToCompletion(composition.Execute);
        Assert.Equal(35d, life.Current);
        Assert.Equal(60d, restoredLife.Current);

        // A has finished before B starts: stale handler bindings become visible.
        var restoredResult = restoredRunner.RunToCompletion(
            restoredComposition.Execute);

        Assert.Equal(result, restoredResult);
        Assert.Equal(SimulationRunStatus.Completed, restoredResult.Status);
        Assert.Equal(new SimulationTime(200L), restoredResult.CurrentTime);
        Assert.Equal(4UL, restoredResult.ProcessedEvents);
        Assert.Equal(0, restoredResult.PendingEvents);
        Assert.Equal(trace, restoredTrace);
        Assert.Equal(observations, restoredObservations);
        Assert.Equal(
            new[]
            {
                nameof(DamageCommittedEvent),
                nameof(ApplyResolvedDamageAction),
                nameof(DamageCommittedEvent)
            },
            trace.Select(entry => entry.Kind).ToArray());
        Assert.Equal(pending[0].Key, trace[0].Key);
        Assert.Equal(secondKey, trace[1].Key);
        Assert.Equal(4UL, trace[2].Key.Sequence.Value);

        Assert.Equal(2, observations.Count);
        Assert.Equal(60d, observations[0].CurrentLife);
        Assert.Equal(1UL, observations[0].LifeRevision);
        Assert.Equal(firstAction.Resolution.RelatedHitExecutionId,
            observations[0].RelatedHitExecutionId);
        Assert.Equal(35d, observations[1].CurrentLife);
        Assert.Equal(2UL, observations[1].LifeRevision);
        Assert.Null(observations[1].RelatedHitExecutionId);
        Assert.Equal(35d, life.Current);
        Assert.Equal(35d, restoredLife.Current);
        Assert.Equal(2UL, life.Revision);
        Assert.Equal(2UL, restoredLife.Revision);

        Assert.Equal(ledgerPrefixCount + 1, ledger.Count);
        Assert.Equal(1, restoredLedger.Count);
        var originalLoss = Assert.IsType<ResourceLossLedgerEntry>(
            ledger.Entries[ledgerPrefixCount]);
        var restoredLoss = Assert.IsType<ResourceLossLedgerEntry>(
            restoredLedger.Entries[0]);
        Assert.Equal(25d, originalLoss.Result.ActualLoss);
        Assert.Equal(originalLoss.Result.ActualLoss,
            restoredLoss.Result.ActualLoss);
        Assert.Equal(originalLoss.Provenance, restoredLoss.Provenance);
    }

    [Theory]
    [InlineData(40d, false,
        DefeatAwareResourceTransactionCommitOutcome.NoDefeatTransition)]
    [InlineData(100d, false,
        DefeatAwareResourceTransactionCommitOutcome.DefeatAccepted)]
    [InlineData(100d, true,
        DefeatAwareResourceTransactionCommitOutcome.DefeatPrevented)]
    public void CommittedEvent_RestoreCopiesFactsWithoutReapplyingCommit(
        double damage,
        bool preventDefeat,
        DefeatAwareResourceTransactionCommitOutcome expectedOutcome)
    {
        var (runtime, sourceId, targetId, lifeId) = CreateRuntime();
        var action = CreateDamage(
            runtime, sourceId, targetId, damage, new SimulationTime(17L), true);
        var ledger = new ResourceOperationLedger();
        var original = CommitDamage(
            runtime, lifeId, ledger, action, preventDefeat);
        var life = runtime.Entities.Get(targetId).Resources.Get(lifeId);
        var current = life.Current;
        var revision = life.Revision;
        var ledgerCount = ledger.Count;

        Assert.Equal(expectedOutcome, original.Outcome);
        var snapshot = CombatWorkItemSnapshotCodec.Capture(original);

        // A historical fact requires no live entity or resolution to rehydrate.
        var emptyRuntime = new SimulationRuntimeState(
            new SimulationSeed(999UL), runtime.ResourceRegistry);
        var restored = Assert.IsType<DamageCommittedEvent>(
            CombatWorkItemSnapshotCodec.Restore(snapshot, emptyRuntime));
        var restoredAgain = Assert.IsType<DamageCommittedEvent>(
            CombatWorkItemSnapshotCodec.Restore(snapshot, emptyRuntime));

        Assert.NotSame(original, restored);
        Assert.NotSame(restored, restoredAgain);
        Assert.Equal(original.DamageExecutionId, restored.DamageExecutionId);
        Assert.Equal(original.GameplayExecutionId, restored.GameplayExecutionId);
        Assert.Equal(original.SourceEntityId, restored.SourceEntityId);
        Assert.Equal(original.TargetEntityId, restored.TargetEntityId);
        Assert.Equal(original.RelatedHitExecutionId, restored.RelatedHitExecutionId);
        Assert.True(restored.IsHitBased);
        Assert.Equal(new SimulationTime(17L), restored.StartedAt);
        Assert.Equal(expectedOutcome, restored.Outcome);
        Assert.Equal(original.DefeatWasAccepted, restored.DefeatWasAccepted);
        Assert.Equal(original.DefeatWasPrevented, restored.DefeatWasPrevented);
        Assert.False(emptyRuntime.Entities.Contains(sourceId));
        Assert.False(emptyRuntime.Entities.Contains(targetId));
        Assert.Equal(current, life.Current);
        Assert.Equal(revision, life.Revision);
        Assert.Equal(ledgerCount, ledger.Count);

        var firstEntity = emptyRuntime.CreateEntity([]);
        Assert.Equal(new EntityId(1UL), firstEntity.Id);
        Assert.Equal(new ExecutionId(1UL), emptyRuntime.CreateExecutionContext(
            firstEntity.Id, SimulationTime.Zero).Id);
    }

    [Fact]
    public void RuntimeSessionSnapshot_ReplayedOrderedInputsProduceSameAuthoritativeContinuation()
    {
        var (runtime, sourceId, targetId, lifeId) =
            CreateRuntime();

        var originalLedger =
            new ResourceOperationLedger();

        var originalTrace =
            new List<(ScheduledEventKey Key, string Kind)>();

        var originalObservations =
            new List<EventObservation>();

        var originalSession =
            new SimulationSession<ISimulationWorkItem>(
                CreateComposition(
                    runtime,
                    lifeId,
                    originalLedger,
                    originalTrace,
                    originalObservations),
                new SimulationSchedulerLimits(
                    maxQueueSize: 20,
                    maxSameTimestampWave: 10),
                new SimulationRunnerLimits(
                    maxProcessedEvents: 20UL));

        //
        // Work already known before the snapshot.
        //
        var firstDamage =
            CreateDamage(
                runtime,
                sourceId,
                targetId,
                amount: 10d,
                new SimulationTime(100L),
                hitBased: true);

        var alreadyPendingDamage =
            CreateDamage(
                runtime,
                sourceId,
                targetId,
                amount: 20d,
                new SimulationTime(200L),
                hitBased: false);

        _ = originalSession.ScheduleExternalInput(
            new SimulationTime(100L),
            firstDamage);

        var alreadyPendingKey =
            originalSession.ScheduleExternalInput(
                new SimulationTime(200L),
                alreadyPendingDamage);

        //
        // First damage commits.
        // Its DamageCommittedEvent is still pending.
        //
        var paused =
            originalSession.RunNext();

        Assert.Equal(
            SimulationRunStatus.InProgress,
            paused.Status);

        var originalLife =
            runtime.Entities.Get(targetId)
                .Resources.Get(lifeId);

        Assert.Equal(
            90d,
            originalLife.Current);

        Assert.Equal(
            1UL,
            originalLife.Revision);

        Assert.Equal(
            1,
            originalLedger.Count);

        Assert.Empty(
            originalObservations);

        //
        // Snapshot contains both kinds of pending Combat work:
        // committed fact + unresolved execution work.
        //
        var snapshot =
            SimulationRuntimeSessionSnapshot<
                CombatWorkItemSnapshot>.Capture<
                    ISimulationWorkItem>(
                        runtime,
                        originalSession,
                        CombatWorkItemSnapshotCodec.Capture);

        Assert.Equal(
            2,
            snapshot.Runner.Scheduler.PendingEvents.Count);

        Assert.IsType<
            CombatWorkItemSnapshot.CommittedDamage>(
                snapshot.Runner.Scheduler
                    .PendingEvents[0].Payload);

        Assert.IsType<
            CombatWorkItemSnapshot.ResolvedDamage>(
                snapshot.Runner.Scheduler
                    .PendingEvents[1].Payload);

        Assert.Equal(
            alreadyPendingKey,
            snapshot.Runner.Scheduler
                .PendingEvents[1].Key);

        var restoredRuntime =
            SimulationRuntimeState.Restore(
                snapshot.Runtime,
                runtime.ResourceRegistry);

        var restoredLedger =
            new ResourceOperationLedger();

        var restoredTrace =
            new List<(ScheduledEventKey Key, string Kind)>();

        var restoredObservations =
            new List<EventObservation>();

        var restoredSession =
            SimulationSession<ISimulationWorkItem>
                .Restore<CombatWorkItemSnapshot>(
                    CreateComposition(
                        restoredRuntime,
                        lifeId,
                        restoredLedger,
                        restoredTrace,
                        restoredObservations),
                    snapshot.Runner,
                    workItemSnapshot =>
                        CombatWorkItemSnapshotCodec.Restore(
                            workItemSnapshot,
                            restoredRuntime));

        //
        // History before the snapshot is already represented
        // by restored authoritative state. This profile does not
        // claim that the old ledger itself is persisted.
        //
        var originalLedgerPrefixCount =
            originalLedger.Count;

        originalTrace.Clear();
        originalObservations.Clear();

        //
        // These are ordered inputs arriving AFTER the snapshot.
        // They contain only replayable input facts; runtime-bound
        // Damage contexts are created by each branch while executing.
        //
        var original150Key =
            originalSession.ScheduleExternalInput(
                new SimulationTime(150L),
                new ReplayDamageInput(
                    sourceId,
                    targetId,
                    Amount: 15d,
                    HitBased: true));

        var restored150Key =
            restoredSession.ScheduleExternalInput(
                new SimulationTime(150L),
                new ReplayDamageInput(
                    sourceId,
                    targetId,
                    Amount: 15d,
                    HitBased: true));

        Assert.Equal(
            original150Key,
            restored150Key);

        var original250Key =
            originalSession.ScheduleExternalInput(
                new SimulationTime(250L),
                new ReplayDamageInput(
                    sourceId,
                    targetId,
                    Amount: 5d,
                    HitBased: false));

        var restored250Key =
            restoredSession.ScheduleExternalInput(
                new SimulationTime(250L),
                new ReplayDamageInput(
                    sourceId,
                    targetId,
                    Amount: 5d,
                    HitBased: false));

        Assert.Equal(
            original250Key,
            restored250Key);

        //
        // Finish A before B. Any accidental closure/reference
        // back into Runtime A becomes visible here.
        //
        var originalResult =
            originalSession.RunToCompletion();

        var restoredLife =
            restoredRuntime.Entities.Get(targetId)
                .Resources.Get(lifeId);

        Assert.Equal(
            50d,
            originalLife.Current);

        Assert.Equal(
            90d,
            restoredLife.Current);

        var restoredResult =
            restoredSession.RunToCompletion();

        Assert.Equal(
            originalResult,
            restoredResult);

        Assert.Equal(
            SimulationRunStatus.Completed,
            restoredResult.Status);

        Assert.Equal(
            new SimulationTime(250L),
            restoredResult.CurrentTime);

        Assert.Equal(
            10UL,
            restoredResult.ProcessedEvents);

        Assert.Equal(
            0,
            restoredResult.PendingEvents);

        //
        // Complete continuation ordering, including the event
        // that was already pending at the snapshot boundary.
        //
        Assert.Equal(
            new[]
            {
            nameof(DamageCommittedEvent),
            nameof(ReplayDamageInput),
            nameof(ApplyResolvedDamageAction),
            nameof(DamageCommittedEvent),
            nameof(ApplyResolvedDamageAction),
            nameof(DamageCommittedEvent),
            nameof(ReplayDamageInput),
            nameof(ApplyResolvedDamageAction),
            nameof(DamageCommittedEvent)
            },
            originalTrace
                .Select(entry => entry.Kind)
                .ToArray());

        Assert.Equal(
            snapshot.Runner.Scheduler
                .PendingEvents[0].Key,
            originalTrace[0].Key);

        Assert.Equal(
            original150Key,
            originalTrace[1].Key);

        Assert.Equal(
            alreadyPendingKey,
            originalTrace[4].Key);

        Assert.Equal(
            original250Key,
            originalTrace[6].Key);

        Assert.Equal(
            originalTrace,
            restoredTrace);

        //
        // Event facts include generated gameplay/damage/hit IDs,
        // commit outcome and the state observed after each commit.
        //
        Assert.Equal(
            originalObservations,
            restoredObservations);

        Assert.Equal(
            new[]
            {
            90d,
            75d,
            55d,
            50d
            },
            originalObservations
                .Select(observation =>
                    observation.CurrentLife)
                .ToArray());

        Assert.Equal(
            new ulong[]
            {
            1UL,
            2UL,
            3UL,
            4UL
            },
            originalObservations
                .Select(observation =>
                    observation.LifeRevision)
                .ToArray());

        Assert.True(
            originalObservations[0]
                .RelatedHitExecutionId.HasValue);

        Assert.True(
            originalObservations[1]
                .RelatedHitExecutionId.HasValue);

        Assert.Null(
            originalObservations[2]
                .RelatedHitExecutionId);

        Assert.Null(
            originalObservations[3]
                .RelatedHitExecutionId);

        //
        // Authoritative state converges independently.
        //
        Assert.Equal(
            50d,
            originalLife.Current);

        Assert.Equal(
            originalLife.Current,
            restoredLife.Current);

        Assert.Equal(
            4UL,
            originalLife.Revision);

        Assert.Equal(
            originalLife.Revision,
            restoredLife.Revision);

        Assert.NotSame(
            originalLife,
            restoredLife);

        //
        // The first ledger entry predates the snapshot.
        // Compare the complete continuation history only.
        //
        Assert.Equal(
            originalLedgerPrefixCount + 3,
            originalLedger.Count);

        Assert.Equal(
            3,
            restoredLedger.Count);

        var expectedLosses =
            new[]
            {
            15d,
            20d,
            5d
            };

        for (var index = 0;
             index < expectedLosses.Length;
             index++)
        {
            var originalLoss =
                Assert.IsType<ResourceLossLedgerEntry>(
                    originalLedger.Entries[
                        originalLedgerPrefixCount + index]);

            var restoredLoss =
                Assert.IsType<ResourceLossLedgerEntry>(
                    restoredLedger.Entries[index]);

            Assert.Equal(
                expectedLosses[index],
                originalLoss.Result.ActualLoss);

            Assert.Equal(
                originalLoss.Result.ActualLoss,
                restoredLoss.Result.ActualLoss);

            Assert.Equal(
                originalLoss.Provenance,
                restoredLoss.Provenance);
        }
    }

    [Fact]
    public void UnknownWorkItem_RejectsCaptureWithoutChangingQueueOrSequence()
    {
        var (runtime, sourceId, targetId, lifeId) = CreateRuntime();
        var action = CreateDamage(
            runtime, sourceId, targetId, 40d, SimulationTime.Zero, false);
        var runner = CreateRunner();
        var firstKey = runner.ScheduleExternalInput(
            new SimulationTime(100L), action);
        var secondKey = runner.ScheduleExternalInput(
            new SimulationTime(200L), new UnsupportedWorkItem());

        Assert.Throws<NotSupportedException>(() => runner.CaptureSnapshot(
            CombatWorkItemSnapshotCodec.Capture));
        Assert.Equal(0UL, runner.ProcessedEvents);
        Assert.Equal(SimulationTime.Zero, runner.CurrentTime);

        var thirdKey = runner.ScheduleExternalInput(
            new SimulationTime(300L), new UnsupportedWorkItem());
        Assert.Equal(3UL, thirdKey.Sequence.Value);
        var keys = new List<ScheduledEventKey>();
        var result = runner.RunToCompletion(context => keys.Add(context.Key));

        Assert.Equal(SimulationRunStatus.Completed, result.Status);
        Assert.Equal(3UL, result.ProcessedEvents);
        Assert.Equal(new[] { firstKey, secondKey, thirdKey }, keys);
        var life = runtime.Entities.Get(targetId).Resources.Get(lifeId);
        Assert.Equal(100d, life.Current);
        Assert.Equal(0UL, life.Revision);
    }

    [Fact]
    public void NullBoundaries_AreRejected()
    {
        var (runtime, sourceId, targetId, _) = CreateRuntime();
        var snapshot = CombatWorkItemSnapshotCodec.Capture(CreateDamage(
            runtime, sourceId, targetId, 40d, SimulationTime.Zero, false));

        Assert.Throws<ArgumentNullException>(() =>
            CombatWorkItemSnapshotCodec.Capture(null!));
        Assert.Throws<ArgumentNullException>(() =>
            DamageCommittedEventSnapshot.Capture(null!));
        Assert.Throws<ArgumentNullException>(() =>
            CombatWorkItemSnapshotCodec.Restore(null!, runtime));
        Assert.Throws<ArgumentNullException>(() =>
            CombatWorkItemSnapshotCodec.Restore(snapshot, null!));
    }

    private static (
        SimulationRuntimeState Runtime,
        EntityId SourceId,
        EntityId TargetId,
        ResourceId LifeId) CreateRuntime()
    {
        var registry = ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse("resource.life"),
                ResourceRole.DamageTarget | ResourceRole.DefeatRelevant)
        ]);
        var lifeId = registry.GetId(ResourceKey.Parse("resource.life"));
        var runtime = new SimulationRuntimeState(
            new SimulationSeed(123UL), registry);
        var source = runtime.CreateEntity([]);
        var target = runtime.CreateEntity(
            [new ResourceState(lifeId, current: 100d, maximum: 100d)]);
        return (runtime, source.Id, target.Id, lifeId);
    }

    private static SimulationRunner<ISimulationWorkItem> CreateRunner()
    {
        return new SimulationRunner<ISimulationWorkItem>(
            new SimulationScheduler<ISimulationWorkItem>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 10, maxSameTimestampWave: 10)),
            new SimulationRunnerLimits(maxProcessedEvents: 20UL));
    }

    private static ApplyResolvedDamageAction CreateDamage(
        SimulationRuntimeState runtime,
        EntityId sourceId,
        EntityId targetId,
        double amount,
        SimulationTime startedAt,
        bool hitBased)
    {
        var execution = runtime.CreateExecutionContext(sourceId, startedAt);
        var damage = runtime.CreateDamageExecutionContext(execution, startedAt);
        var target = hitBased
            ? runtime.CreateDamageTargetContext(damage,
                runtime.CreateHitExecutionContext(execution, targetId, startedAt))
            : runtime.CreateDamageTargetContext(damage, targetId);
        var quantities = new DamageResolutionQuantities(
            new IncomingDamage(amount),
            new PostMitigationDamage(amount),
            new PostTakenScalingDamage(amount),
            new DamageTaken(amount));
        return new ApplyResolvedDamageAction(runtime.CreateDamageResolutionContext(
            damage, target, quantities));
    }

    private static DamageCommittedEvent CommitDamage(
        SimulationRuntimeState runtime,
        ResourceId lifeId,
        ResourceOperationLedger ledger,
        ApplyResolvedDamageAction action,
        bool preventDefeat = false)
    {
        var resolution = action.Resolution;

        // Explicitly check that pending work belongs to this branch's runtime.
        _ = runtime.CreateDamageResolutionContext(
            resolution.Execution, resolution.Target, resolution.Quantities);

        var target = runtime.Entities.Get(resolution.TargetEntityId);
        var lossPlan = new DamageResourceLossPlan(
            new DamageResourceTargetContext(
                resolution, new ResourceStateTarget(target, lifeId)),
            resolution.Quantities.Taken.Amount);
        var owner = new DamageApplicationOwnerPlan(
            target, DefeatRelevantResourcePolicy.AnyDepleted);

        if (preventDefeat)
        {
            var interventionExecution = runtime.CreateExecutionContext(
                resolution.SourceEntityId, resolution.StartedAt);
            owner = new DamageApplicationOwnerPlan(
                target,
                DefeatRelevantResourcePolicy.AnyDepleted,
                interventionExecution.Id,
                context =>
                {
                    PreDefeatRecoveryIntervention.Apply(
                        context, lifeId, recoveryAmount: 25d);
                });
        }

        var result = ResolvedDamageApplicationExecutor.Apply(
            resolution, [lossPlan], [owner], ledger);
        return new DamageCommittedEvent(resolution, result.TargetCommitResult);
    }

    private static SimulationComposition<ISimulationWorkItem> CreateComposition(
        SimulationRuntimeState runtime,
        ResourceId lifeId,
        ResourceOperationLedger ledger,
        List<(ScheduledEventKey Key, string Kind)> trace,
        List<EventObservation> observations)
    {
        var builder = new SimulationCompositionBuilder<ISimulationWorkItem>();
        builder.AddModule("CombatSnapshotProfile", module =>
        {
            module.Handle<ReplayDamageInput>(
    (input, context) =>
    {
        trace.Add(
            (
                context.Key,
                nameof(ReplayDamageInput)
            ));

        //
        // Same replay input + same restored allocator state
        // must produce the same runtime-bound contexts/IDs.
        //
        var action =
            CreateDamage(
                runtime,
                input.SourceEntityId,
                input.TargetEntityId,
                input.Amount,
                context.CurrentTime,
                input.HitBased);

        context.Schedule(
            context.CurrentTime,
            SchedulerPhase.FollowUp,
            action);
    });
            module.Handle<ApplyResolvedDamageAction>((action, context) =>
            {
                trace.Add((context.Key, nameof(ApplyResolvedDamageAction)));
                using var preparedEvent = context.PrepareFollowUp();
                preparedEvent.Publish(CommitDamage(runtime, lifeId, ledger, action));
            });

            module.Handle<DamageCommittedEvent>((domainEvent, context) =>
            {
                trace.Add((context.Key, nameof(DamageCommittedEvent)));
                var life = runtime.Entities.Get(domainEvent.TargetEntityId)
                    .Resources.Get(lifeId);
                observations.Add(new EventObservation(
                    context.Key,
                    domainEvent.DamageExecutionId,
                    domainEvent.GameplayExecutionId,
                    domainEvent.SourceEntityId,
                    domainEvent.TargetEntityId,
                    domainEvent.RelatedHitExecutionId,
                    domainEvent.StartedAt,
                    domainEvent.Outcome,
                    life.Current,
                    life.Revision));
            });
        });
        return builder.Build();
    }

    private sealed record EventObservation(
        ScheduledEventKey Key,
        DamageExecutionId DamageExecutionId,
        ExecutionId GameplayExecutionId,
        EntityId SourceEntityId,
        EntityId TargetEntityId,
        HitExecutionId? RelatedHitExecutionId,
        SimulationTime StartedAt,
        DefeatAwareResourceTransactionCommitOutcome Outcome,
        double CurrentLife,
        ulong LifeRevision);

    private sealed class UnsupportedWorkItem : ISimulationWorkItem
    {
    }

    private sealed record ReplayDamageInput(
    EntityId SourceEntityId,
    EntityId TargetEntityId,
    double Amount,
    bool HitBased)
    : ISimulationWorkItem;
}