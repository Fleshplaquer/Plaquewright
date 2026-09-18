using Plaquewright.Core.Combat;
using Plaquewright.Core.Composition;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

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
}