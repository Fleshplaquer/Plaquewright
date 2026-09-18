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
        ISimulationWorkItem>(
        new ObserveCommittedDamageReaction());

        var builder =
    new SimulationCompositionBuilder<
        ISimulationWorkItem>();

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
        resolution,
        commitResult));
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
                            new EntityId(1UL),
                            domainEvent.SourceEntityId);

                        Assert.Null(
                            domainEvent.RelatedHitExecutionId);

                        Assert.False(
                            domainEvent.IsHitBased);

                        Assert.Equal(
                            new SimulationTime(100L),
                            domainEvent.StartedAt);

                        Assert.Equal(
                            DefeatAwareResourceTransactionCommitOutcome
                                .DefeatPrevented,
                            domainEvent.Outcome);

                        Assert.True(
                            domainEvent.DefeatWasPrevented);

                        Assert.False(
                            domainEvent.DefeatWasAccepted);

                        Assert.Equal(
                            DefeatAwareResourceTransactionCommitOutcome
                                .DefeatPrevented,
                            domainEvent.Outcome);

                        reactionDispatcher.Dispatch(
                            domainEvent,
                            new DomainReactionContext<
    ISimulationWorkItem>(
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
            new SimulationSession<ISimulationWorkItem>(
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
    [Fact]
    public void DamageCommittedEvent_WhenResolutionAndCommitTargetDiffer_IsRejected()
    {
        var setup =
            CreateSetup();

        var ledger =
            new ResourceOperationLedger();

        var committedResolution =
            CreateDamageResolution(
                setup.Entity.Id,
                damageTakenAmount: 100d,
                SimulationTime.Zero,
                new DamageExecutionId(10UL),
                new ExecutionId(10UL));

        var damageTarget =
            new DamageResourceTargetContext(
                committedResolution,
                setup.LifeTarget);

        var lossPlan =
            new DamageResourceLossPlan(
                damageTarget,
                requestedResourceLoss: 100d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        DamageResourceTransactionStager.Stage(
            draft,
            lossPlan);

        var preDefeatContext =
            PreDefeatInterventionContextFactory
                .TryCreate(
                    draft,
                    setup.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted,
                    new ExecutionId(11UL));

        Assert.NotNull(
            preDefeatContext);

        var intervention =
            PreDefeatRecoveryIntervention.Apply(
                preDefeatContext,
                setup.LifeId,
                recoveryAmount: 25d);

        Assert.True(
            intervention.WasDefeatResolved);

        var phaseResult =
            PreDefeatInterventionPhaseFinalizer
                .Finalize(
                    preDefeatContext);

        var commitResult =
            DefeatAwareResourceTransactionCommitter
                .Commit(
                    draft,
                    ledger,
                    setup.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted,
                    phaseResult);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome
                .DefeatPrevented,
            commitResult.Outcome);

        //
        // Same committed result, but a resolution that claims
        // a different target entity.
        //
        var mismatchedResolution =
            CreateDamageResolution(
                new EntityId(999UL),
                damageTakenAmount: 100d,
                SimulationTime.Zero,
                new DamageExecutionId(12UL),
                new ExecutionId(12UL));

        Assert.Throws<InvalidOperationException>(
            () =>
                new DamageCommittedEvent(
                    mismatchedResolution,
                    commitResult));
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

    private sealed class ResolveLethalDamageAction
        : ISimulationWorkItem
    {
    }

    private sealed class ObserveCommittedDamageAction
        : ISimulationWorkItem
    {
    }


    private sealed class ObserveCommittedDamageReaction
    : IDomainReaction<
        DamageCommittedEvent,
        ISimulationWorkItem>
    {
        public void React(
    DamageCommittedEvent domainEvent,
    DomainReactionContext<ISimulationWorkItem> context)
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