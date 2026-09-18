using Plaquewright.Core.Combat;
using Plaquewright.Core.Composition;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class CombatPendingWorkSnapshotTests
{
    [Fact]
    public void PendingResolvedDamage_RestoreIntoNewRuntime_ProducesSameContinuation()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var originalRuntime =
            new SimulationRuntimeState(
                new SimulationSeed(123UL),
                registry);

        var source =
            originalRuntime.CreateEntity(
                []);

        var target =
            originalRuntime.CreateEntity(
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var startedAt =
            new SimulationTime(
                10L);

        var execution =
            originalRuntime.CreateExecutionContext(
                source.Id,
                startedAt);

        var damageExecution =
            originalRuntime.CreateDamageExecutionContext(
                execution,
                startedAt);

        var damageTarget =
            originalRuntime.CreateDamageTargetContext(
                damageExecution,
                target.Id);

        var quantities =
            new DamageResolutionQuantities(
                new IncomingDamage(
                    40d),
                new PostMitigationDamage(
                    40d),
                new PostTakenScalingDamage(
                    40d),
                new DamageTaken(
                    40d));

        var resolution =
            originalRuntime.CreateDamageResolutionContext(
                damageExecution,
                damageTarget,
                quantities);

        var action =
            new ApplyResolvedDamageAction(
                resolution);

        var originalRunner =
            new SimulationRunner<ISimulationWorkItem>(
                new SimulationScheduler<ISimulationWorkItem>(
                    new SimulationSchedulerLimits(
                        maxQueueSize: 10,
                        maxSameTimestampWave: 10)),
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        var scheduledKey =
            originalRunner.ScheduleExternalInput(
                new SimulationTime(100L),
                action);

        //
        // Capture both halves of the authoritative
        // continuation before the pending action runs.
        //
        var runtimeSnapshot =
            originalRuntime.CaptureSnapshot();

        var runnerSnapshot =
            originalRunner.CaptureSnapshot(
                workItem =>
                {
                    var damageAction =
                        Assert.IsType<
                            ApplyResolvedDamageAction>(
                            workItem);

                    return ApplyResolvedDamageActionSnapshot
                        .Capture(
                            damageAction);
                });

        var restoredRuntime =
            SimulationRuntimeState.Restore(
                runtimeSnapshot,
                registry);

        var restoredRunner =
            SimulationRunner<ISimulationWorkItem>
                .Restore(
                    runnerSnapshot,
                    snapshot =>
                        snapshot.Restore(
                            restoredRuntime));

        var originalLedger =
            new ResourceOperationLedger();

        var restoredLedger =
            new ResourceOperationLedger();

        var originalTrace =
            new List<ScheduledEventKey>();

        var restoredTrace =
            new List<ScheduledEventKey>();

        var originalComposition =
            CreateComposition(
                originalRuntime,
                lifeId,
                originalLedger,
                originalTrace);

        var restoredComposition =
            CreateComposition(
                restoredRuntime,
                lifeId,
                restoredLedger,
                restoredTrace);

        //
        // Runtime A continues normally.
        //
        var originalResult =
            originalRunner.RunToCompletion(
                originalComposition.Execute);

        //
        // Runtime B continues from captured state
        // and reconstructed pending work.
        //
        var restoredResult =
            restoredRunner.RunToCompletion(
                restoredComposition.Execute);

        Assert.Equal(
            originalResult,
            restoredResult);

        Assert.Equal(
            SimulationRunStatus.Completed,
            restoredResult.Status);

        Assert.Equal(
            new SimulationTime(100L),
            restoredResult.CurrentTime);

        Assert.Equal(
            1UL,
            restoredResult.ProcessedEvents);

        Assert.Equal(
            0,
            restoredResult.PendingEvents);

        Assert.Equal(
            new[]
            {
                scheduledKey
            },
            originalTrace);

        Assert.Equal(
            originalTrace,
            restoredTrace);

        var originalTarget =
            originalRuntime.Entities.Get(
                target.Id);

        var restoredTarget =
            restoredRuntime.Entities.Get(
                target.Id);

        var originalLife =
            originalTarget.Resources.Get(
                lifeId);

        var restoredLife =
            restoredTarget.Resources.Get(
                lifeId);

        Assert.Equal(
            60d,
            originalLife.Current);

        Assert.Equal(
            originalLife.Current,
            restoredLife.Current);

        Assert.Equal(
            1UL,
            originalLife.Revision);

        Assert.Equal(
            originalLife.Revision,
            restoredLife.Revision);

        Assert.NotSame(
            originalTarget,
            restoredTarget);

        Assert.NotSame(
            originalLife,
            restoredLife);

        Assert.Equal(
            1,
            originalLedger.Count);

        Assert.Equal(
            originalLedger.Count,
            restoredLedger.Count);

        var originalLoss =
            Assert.IsType<ResourceLossLedgerEntry>(
                originalLedger.Entries[0]);

        var restoredLoss =
            Assert.IsType<ResourceLossLedgerEntry>(
                restoredLedger.Entries[0]);

        Assert.Equal(
            40d,
            originalLoss.Result.ActualLoss);

        Assert.Equal(
            originalLoss.Result.ActualLoss,
            restoredLoss.Result.ActualLoss);

        Assert.Equal(
            originalLoss.Provenance,
            restoredLoss.Provenance);
    }

    private static SimulationComposition<
        ISimulationWorkItem>
        CreateComposition(
            SimulationRuntimeState runtime,
            ResourceId lifeId,
            ResourceOperationLedger ledger,
            List<ScheduledEventKey> trace)
    {
        var builder =
            new SimulationCompositionBuilder<
                ISimulationWorkItem>();

        builder.AddModule(
            "CombatSnapshotProbe",
            module =>
            {
                module.Handle<
                    ApplyResolvedDamageAction>(
                    (action, context) =>
                    {
                        trace.Add(
                            context.Key);

                        var target =
                            runtime.Entities.Get(
                                action.Resolution
                                    .TargetEntityId);

                        var lifeTarget =
                            new ResourceStateTarget(
                                target,
                                lifeId);

                        var damageTarget =
                            new DamageResourceTargetContext(
                                action.Resolution,
                                lifeTarget);

                        var lossPlan =
                            new DamageResourceLossPlan(
                                damageTarget,
                                action.Resolution
                                    .Quantities
                                    .Taken
                                    .Amount);

                        var result =
                            ResolvedDamageApplicationExecutor
                                .Apply(
                                    action.Resolution,
                                    [
                                        lossPlan
                                    ],
                                    [
                                        new DamageApplicationOwnerPlan(
                                            target,
                                            DefeatRelevantResourcePolicy
                                                .AnyDepleted)
                                    ],
                                    ledger);

                        Assert.Equal(
                            DefeatAwareResourceTransactionCommitOutcome
                                .NoDefeatTransition,
                            result.TargetCommitResult
                                .Outcome);
                    });
            });

        return builder.Build();
    }

    private static CompiledResourceRegistry
        CreateRegistry()
    {
        return ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant)
        ]);
    }
}