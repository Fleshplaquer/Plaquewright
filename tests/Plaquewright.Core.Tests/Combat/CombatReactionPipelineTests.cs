using Plaquewright.Core.Combat;
using Plaquewright.Core.Composition;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Events;
using Plaquewright.Core.Hosting;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class CombatReactionPipelineTests
{
    [Fact]
    public void LethalDamage_WithPreDefeatRecovery_CommitsBeforePostCommitReaction()
    {
        var setup =
            CreateSetup();

        var ledger =
            new ResourceOperationLedger();

        var trace =
            new List<string>();

        var waves =
            new List<uint>();

        var observedLife =
            double.NaN;

        var observedLedgerCount =
            -1;

        var commitOutcome =
            default(
                DefeatAwareResourceTransactionCommitOutcome);

        var damageExecutionId =
            new DamageExecutionId(
                1UL);

        var damageGameplayExecutionId =
            new ExecutionId(
                1UL);

        var interventionExecutionId =
            new ExecutionId(
                2UL);

        var reactionDispatcher =
            new DomainReactionDispatcher<
                DamageCommittedEvent,
                CombatWorkItem>(
                new ObserveCommittedDamageReaction());

        var builder =
            new SimulationCompositionBuilder<
                CombatWorkItem>();

        builder.AddModule(
            "CombatReference",
            module =>
            {
                module.Handle<ResolveLethalDamageAction>(
                    (_, context) =>
                    {
                        trace.Add(
                            nameof(
                                ResolveLethalDamageAction));

                        waves.Add(
                            context.Key.Wave.Value);

                        var resolution =
                            CreateDamageResolution(
                                setup.Entity.Id,
                                damageTakenAmount: 100d,
                                context.CurrentTime,
                                damageExecutionId,
                                damageGameplayExecutionId);

                        var damageTarget =
                            new DamageResourceTargetContext(
                                resolution,
                                setup.LifeTarget);

                        var lossPlan =
                            new DamageResourceLossPlan(
                                damageTarget,
                                requestedResourceLoss: 100d);

                        var draft =
                            new ResourceTransactionDraft(
                                setup.Registry);

                        var preview =
                            DamageResourceTransactionStager.Stage(
                                draft,
                                lossPlan);

                        Assert.Equal(
                            100d,
                            preview.Result.ActualLoss);

                        //
                        // Lethal projection exists,
                        // but authoritative state is untouched.
                        //
                        Assert.Equal(
                            0d,
                            draft.GetProjectedValues(
                                setup.LifeTarget)
                            .Current);

                        Assert.Equal(
                            100d,
                            setup.LifeTarget.State.Current);

                        Assert.Equal(
                            0UL,
                            setup.LifeTarget.State.Revision);

                        var preDefeatContext =
                            PreDefeatInterventionContextFactory
                                .TryCreate(
                                    draft,
                                    setup.Entity,
                                    DefeatRelevantResourcePolicy
                                        .AnyDepleted,
                                    interventionExecutionId);

                        Assert.NotNull(
                            preDefeatContext);

                        var intervention =
                            PreDefeatRecoveryIntervention.Apply(
                                preDefeatContext,
                                setup.LifeId,
                                recoveryAmount: 25d);

                        Assert.True(
                            intervention.WasDefeatResolved);

                        //
                        // PreDefeat changes the still-uncommitted
                        // transaction result.
                        //
                        Assert.Equal(
                            25d,
                            draft.GetProjectedValues(
                                setup.LifeTarget)
                            .Current);

                        Assert.Equal(
                            100d,
                            setup.LifeTarget.State.Current);

                        var phaseResult =
                            PreDefeatInterventionPhaseFinalizer
                                .Finalize(
                                    preDefeatContext);

                        Assert.Equal(
                            PreDefeatInterventionPhaseOutcome
                                .Resolved,
                            phaseResult.Outcome);

                        //
                        // Reserve publication BEFORE authoritative
                        // state becomes visible.
                        //
                        // The event contains only information already
                        // known before commit. It becomes a true
                        // "committed" event only when Publish() runs
                        // after the successful commit.
                        //
                        using var preparedEvent =
     context.PrepareFollowUp();

                        var commitResult =
                            DefeatAwareResourceTransactionCommitter
                                .Commit(
                                    draft,
                                    ledger,
                                    setup.Entity,
                                    DefeatRelevantResourcePolicy
                                        .AnyDepleted,
                                    phaseResult);

                        commitOutcome =
                            commitResult.Outcome;

                        Assert.Equal(
                            DefeatAwareResourceTransactionCommitOutcome
                                .DefeatPrevented,
                            commitResult.Outcome);

                        Assert.Equal(
                            25d,
                            setup.LifeTarget.State.Current);

                        Assert.Equal(
                            1UL,
                            setup.LifeTarget.State.Revision);

                        Assert.Equal(
                            2,
                            ledger.Count);

                        //
                        // The event is constructed from the actual
                        // authoritative commit result.
                        //
                        preparedEvent.Publish(
                            new DamageCommittedEvent(
                                resolution.DamageExecutionId,
                                resolution.GameplayExecutionId,
                                resolution.TargetEntityId,
                                setup.LifeId,
                                commitResult.Outcome));
                    });

                module.Handle<DamageCommittedEvent>(
                    (domainEvent, context) =>
                    {
                        trace.Add(
                            nameof(
                                DamageCommittedEvent));

                        waves.Add(
                            context.Key.Wave.Value);

                        //
                        // Event execution occurs only after
                        // the Combat commit is visible.
                        //
                        Assert.Equal(
                            25d,
                            setup.LifeTarget.State.Current);

                        Assert.Equal(
                            1UL,
                            setup.LifeTarget.State.Revision);

                        Assert.Equal(
                            2,
                            ledger.Count);

                        Assert.Equal(
                            damageExecutionId,
                            domainEvent.DamageExecutionId);

                        Assert.Equal(
                            damageGameplayExecutionId,
                            domainEvent.GameplayExecutionId);

                        Assert.Equal(
                            setup.Entity.Id,
                            domainEvent.TargetEntityId);

                        Assert.Equal(
                            setup.LifeId,
                            domainEvent.ResourceId);

                        Assert.Equal(
                            DefeatAwareResourceTransactionCommitOutcome
                                .DefeatPrevented,
                            domainEvent.Outcome);

                        reactionDispatcher.Dispatch(
                            domainEvent,
                            new DomainReactionContext<
                                CombatWorkItem>(
                                context));
                    });

                module.Handle<ObserveCommittedDamageAction>(
                    (_, context) =>
                    {
                        trace.Add(
                            nameof(
                                ObserveCommittedDamageAction));

                        waves.Add(
                            context.Key.Wave.Value);

                        observedLife =
                            setup.LifeTarget.State.Current;

                        observedLedgerCount =
                            ledger.Count;
                    });
            });

        var session =
            new SimulationSession<CombatWorkItem>(
                builder.Build(),
                new SimulationSchedulerLimits(
                    maxQueueSize: 100,
                    maxSameTimestampWave: 10),
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        session.ScheduleExternalInput(
            new SimulationTime(100L),
            new ResolveLethalDamageAction());

        var result =
            session.RunToCompletion();

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.Equal(
            3UL,
            session.ProcessedEvents);

        Assert.Equal(
            new[]
            {
                nameof(ResolveLethalDamageAction),
                nameof(DamageCommittedEvent),
                nameof(ObserveCommittedDamageAction)
            },
            trace);

        Assert.Equal(
            new uint[]
            {
                0u,
                1u,
                2u
            },
            waves);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome
                .DefeatPrevented,
            commitOutcome);

        Assert.Equal(
            25d,
            observedLife);

        Assert.Equal(
            2,
            observedLedgerCount);

        //
        // Causal provenance must survive the complete
        // scheduler/event/reaction path.
        //
        var damageEntry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            new ResourceOperationProvenance(
                ResourceOperationCause.DamageDerived,
                damageGameplayExecutionId),
            damageEntry.Provenance);

        var recoveryEntry =
            Assert.IsType<ResourceRecoveryLedgerEntry>(
                ledger.Entries[1]);

        Assert.Equal(
            new ResourceOperationProvenance(
                ResourceOperationCause.Recovery,
                interventionExecutionId),
            recoveryEntry.Provenance);
    }

    private static DamageResolutionContext
        CreateDamageResolution(
            EntityId targetEntityId,
            double damageTakenAmount,
            SimulationTime simulationTime,
            DamageExecutionId damageExecutionId,
            ExecutionId gameplayExecutionId)
    {
        var damageExecution =
            new DamageExecutionContext(
                damageExecutionId,
                gameplayExecutionId,
                new EntityId(1UL),
                simulationTime);

        var damageTarget =
            new DamageTargetContext(
                damageExecution.Id,
                targetEntityId,
                relatedHitExecutionId: null);

        var quantities =
            DamageResolutionTestFactory.Create(
                new IncomingDamage(
                    damageTakenAmount),
                postMitigationAmount:
                    damageTakenAmount,
                postTakenScalingAmount:
                    damageTakenAmount,
                damageTakenAmount:
                    damageTakenAmount);

        return new DamageResolutionContext(
            damageExecution,
            damageTarget,
            quantities);
    }

    private static TestSetup CreateSetup()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.life"),
                    ResourceRole.DamageTarget |
                    ResourceRole.DefeatRelevant)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var entity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        return new TestSetup(
            registry,
            lifeId,
            entity,
            new ResourceStateTarget(
                entity,
                lifeId));
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId LifeId,
        EntityRuntimeState Entity,
        ResourceStateTarget LifeTarget);

    private abstract class CombatWorkItem
    {
    }

    private sealed class ResolveLethalDamageAction
        : CombatWorkItem
    {
    }

    private sealed class DamageCommittedEvent
        : CombatWorkItem,
          IDomainEvent
    {
        public DamageExecutionId DamageExecutionId { get; }

        public ExecutionId GameplayExecutionId { get; }

        public EntityId TargetEntityId { get; }

        public ResourceId ResourceId { get; }
        public DefeatAwareResourceTransactionCommitOutcome Outcome { get; }

        public DamageCommittedEvent(
    DamageExecutionId damageExecutionId,
    ExecutionId gameplayExecutionId,
    EntityId targetEntityId,
    ResourceId resourceId,
    DefeatAwareResourceTransactionCommitOutcome outcome)
        {
            DamageExecutionId =
                damageExecutionId;

            GameplayExecutionId =
                gameplayExecutionId;

            TargetEntityId =
                targetEntityId;

            ResourceId =
                resourceId;

            Outcome =
                outcome;
        }
    }

    private sealed class ObserveCommittedDamageAction
        : CombatWorkItem
    {
    }

    private sealed class ObserveCommittedDamageReaction
        : IDomainReaction<
            DamageCommittedEvent,
            CombatWorkItem>
    {
        public void React(
            DamageCommittedEvent domainEvent,
            DomainReactionContext<CombatWorkItem> context)
        {
            ArgumentNullException.ThrowIfNull(
                domainEvent);

            ArgumentNullException.ThrowIfNull(
                context);

            context.ScheduleFollowUp(
                new ObserveCommittedDamageAction());
        }
    }
}