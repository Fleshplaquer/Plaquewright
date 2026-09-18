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
        var resolution =
CreateDamageResolution(
setup.Entity.Id,
damageTakenAmount: 100d,
new SimulationTime(100L),
damageExecutionId,
damageGameplayExecutionId);

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
                module.Handle<ApplyResolvedDamageAction>(
    (action, context) =>
    {
        trace.Add(
            nameof(
                ApplyResolvedDamageAction));

        waves.Add(
            context.Key.Wave.Value);

        var appliedResolution =
            action.Resolution;

        var damageTarget =
            new DamageResourceTargetContext(
                appliedResolution,
                setup.LifeTarget);

        var lossPlan =
            new DamageResourceLossPlan(
                damageTarget,
                requestedResourceLoss: 100d);

        using var preparedEvent =
    context.PrepareFollowUp();
        var applicationResult =
        ResolvedDamageApplicationExecutor.Apply(
            appliedResolution,
            [
                lossPlan
            ],
            [
                new DamageApplicationOwnerPlan(
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted,
                interventionExecutionId,
                preDefeatContext =>
                {
                    var intervention =
                        PreDefeatRecoveryIntervention.Apply(
                            preDefeatContext,
                            setup.LifeId,
                            recoveryAmount: 25d);

                    Assert.True(
                        intervention.WasDefeatResolved);
                })
            ],
            ledger);
        var commitResult =
applicationResult.TargetCommitResult;

        commitOutcome =
            commitResult.Outcome;



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

        var preview =
Assert.Single(
    applicationResult.ResourceLossPreviews);

        Assert.Equal(
            100d,
            preview.Result.ActualLoss);

        //
        // The event is constructed from the actual
        // authoritative commit result.
        //
        preparedEvent.Publish(
            new DamageCommittedEvent(
            appliedResolution,
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
    new ApplyResolvedDamageAction(
        resolution));

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
        nameof(ApplyResolvedDamageAction),
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
    public void ResolvedDamageApplication_WhenLossPlanBelongsToDifferentResolution_IsRejected()
    {
        var setup =
            CreateSetup();

        var ledger =
            new ResourceOperationLedger();

        var expectedResolution =
            CreateDamageResolution(
                setup.Entity.Id,
                damageTakenAmount: 100d,
                SimulationTime.Zero,
                new DamageExecutionId(20UL),
                new ExecutionId(20UL));

        var foreignResolution =
            CreateDamageResolution(
                setup.Entity.Id,
                damageTakenAmount: 100d,
                SimulationTime.Zero,
                new DamageExecutionId(21UL),
                new ExecutionId(21UL));

        var foreignTarget =
            new DamageResourceTargetContext(
                foreignResolution,
                setup.LifeTarget);

        var foreignPlan =
            new DamageResourceLossPlan(
                foreignTarget,
                requestedResourceLoss: 100d);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResolvedDamageApplicationExecutor.Apply(
                    expectedResolution,
                    [
                        foreignPlan
                    ],
                    [
                        new DamageApplicationOwnerPlan(
                        setup.Entity,
                        DefeatRelevantResourcePolicy.AnyDepleted)
                    ],
                    ledger));

        //
        // Rejection happens before authoritative mutation.
        //
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);
    }

    [Fact]
    public void ResolvedDamageApplication_WhenTargetOwnerIsMissing_IsRejectedBeforeCommit()
    {
        var setup =
            CreateSetup();

        var ledger =
            new ResourceOperationLedger();

        var resolution =
            CreateDamageResolution(
                setup.Entity.Id,
                damageTakenAmount: 100d,
                SimulationTime.Zero,
                new DamageExecutionId(30UL),
                new ExecutionId(30UL));

        var damageTarget =
            new DamageResourceTargetContext(
                resolution,
                setup.LifeTarget);

        var lossPlan =
            new DamageResourceLossPlan(
                damageTarget,
                requestedResourceLoss: 100d);

        //
        // Same registry, but deliberately the wrong entity owner.
        //
        var unrelatedEntity =
            new EntityRuntimeState(
                new EntityId(999UL),
                setup.Registry,
                [
                    new ResourceState(
                    setup.LifeId,
                    current: 100d,
                    maximum: 100d)
                ]);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResolvedDamageApplicationExecutor.Apply(
                    resolution,
                    [
                        lossPlan
                    ],
                    [
                        new DamageApplicationOwnerPlan(
                        unrelatedEntity,
                        DefeatRelevantResourcePolicy.AnyDepleted)
                    ],
                    ledger));

        //
        // Draft ownership rejection occurs before commit.
        //
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);
    }
    [Fact]
    public void PaidAttack_CommitsCostBeforeResolvedDamageAndKeepsOperationsDistinct()
    {
        var setup =
            CreatePaidAttackSetup();

        var ledger =
            new ResourceOperationLedger();

        var trace =
            new List<string>();

        var waves =
            new List<uint>();

        var costExecutionId =
            new ExecutionId(
                40UL);

        var damageExecutionId =
            new DamageExecutionId(
                41UL);

        var damageGameplayExecutionId =
            new ExecutionId(
                41UL);

        var payMana =
            new ResourceCostTransactionParticipant(
                setup.ManaTarget,
                new ResourceCostRequest(
                    setup.ManaId,
                    amount: 30d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct,
                        costExecutionId)),
                ledger);

        var costReaction =
            new DomainReactionDispatcher<
                AttackCostCommittedEvent,
                ISimulationWorkItem>(
                new ResolvePaidAttackDamageReaction());

        var damageReaction =
            new DomainReactionDispatcher<
                DamageCommittedEvent,
                ISimulationWorkItem>(
                new ObserveCommittedDamageReaction());

        var builder =
            new SimulationCompositionBuilder<
                ISimulationWorkItem>();

        builder.AddModule(
            "PaidCombatReference",
            module =>
            {
                module.Handle<PayAttackCostAction>(
                    (_, context) =>
                    {
                        trace.Add(
                            nameof(PayAttackCostAction));

                        waves.Add(
                            context.Key.Wave.Value);

                        var committed =
                            DomainEventTransactionCoordinator
                                .TryCommitAndPublish<
                                    ISimulationWorkItem,
                                    AttackCostCommittedEvent>(
                                    context,
                                    new AttackCostCommittedEvent(),
                                    payMana);

                        Assert.True(
                            committed);

                        //
                        // Cost is authoritative before the
                        // follow-up event can execute.
                        //
                        Assert.Equal(
                            70d,
                            setup.ManaTarget.State.Current);

                        Assert.Equal(
                            1UL,
                            setup.ManaTarget.State.Revision);

                        Assert.Equal(
                            100d,
                            setup.LifeTarget.State.Current);

                        Assert.Equal(
                            0UL,
                            setup.LifeTarget.State.Revision);

                        Assert.Equal(
                            1,
                            ledger.Count);
                    });

                module.Handle<AttackCostCommittedEvent>(
                    (domainEvent, context) =>
                    {
                        trace.Add(
                            nameof(AttackCostCommittedEvent));

                        waves.Add(
                            context.Key.Wave.Value);

                        Assert.Equal(
                            70d,
                            setup.ManaTarget.State.Current);

                        Assert.Equal(
                            100d,
                            setup.LifeTarget.State.Current);

                        var costEntry =
                            Assert.IsType<ResourceCostLedgerEntry>(
                                ledger.Entries[0]);

                        Assert.Equal(
                            30d,
                            costEntry.Result.ActualCost);

                        costReaction.Dispatch(
                            domainEvent,
                            new DomainReactionContext<
                                ISimulationWorkItem>(
                                context));
                    });

                module.Handle<ResolvePaidAttackDamageAction>(
                    (_, context) =>
                    {
                        trace.Add(
                            nameof(
                                ResolvePaidAttackDamageAction));

                        waves.Add(
                            context.Key.Wave.Value);

                        //
                        // Damage is resolved only after the
                        // cost has already committed.
                        //
                        Assert.Equal(
                            70d,
                            setup.ManaTarget.State.Current);

                        var resolution =
                            CreateDamageResolution(
                                setup.Target.Id,
                                damageTakenAmount: 40d,
                                context.CurrentTime,
                                damageExecutionId,
                                damageGameplayExecutionId);

                        context.Schedule(
                            context.CurrentTime,
                            SchedulerPhase.FollowUp,
                            new ApplyResolvedDamageAction(
                                resolution));
                    });

                module.Handle<ApplyResolvedDamageAction>(
                    (action, context) =>
                    {
                        trace.Add(
                            nameof(
                                ApplyResolvedDamageAction));

                        waves.Add(
                            context.Key.Wave.Value);

                        var resolution =
                            action.Resolution;

                        var damageTarget =
                            new DamageResourceTargetContext(
                                resolution,
                                setup.LifeTarget);

                        var lossPlan =
                            new DamageResourceLossPlan(
                                damageTarget,
                                requestedResourceLoss: 40d);

                        //
                        // Publication capacity is guaranteed
                        // before Combat makes state visible.
                        //
                        using var preparedEvent =
                            context.PrepareFollowUp();

                        var applicationResult =
                            ResolvedDamageApplicationExecutor.Apply(
                                resolution,
                                [
                                    lossPlan
                                ],
                                [
                                    new DamageApplicationOwnerPlan(
                                    setup.Target,
                                    DefeatRelevantResourcePolicy
                                        .AnyDepleted)
                                ],
                                ledger);

                        var commitResult =
                            applicationResult.TargetCommitResult;

                        Assert.Equal(
                            DefeatAwareResourceTransactionCommitOutcome
                                .NoDefeatTransition,
                            commitResult.Outcome);

                        Assert.Equal(
                            70d,
                            setup.ManaTarget.State.Current);

                        Assert.Equal(
                            60d,
                            setup.LifeTarget.State.Current);

                        Assert.Equal(
                            2,
                            ledger.Count);

                        preparedEvent.Publish(
                            new DamageCommittedEvent(
                                resolution,
                                commitResult));
                    });

                module.Handle<DamageCommittedEvent>(
                    (domainEvent, context) =>
                    {
                        trace.Add(
                            nameof(DamageCommittedEvent));

                        waves.Add(
                            context.Key.Wave.Value);

                        //
                        // Cost and damage remain distinct
                        // authoritative resource operations.
                        //
                        Assert.Equal(
                            70d,
                            setup.ManaTarget.State.Current);

                        Assert.Equal(
                            60d,
                            setup.LifeTarget.State.Current);

                        Assert.Equal(
                            1UL,
                            setup.ManaTarget.State.Revision);

                        Assert.Equal(
                            1UL,
                            setup.LifeTarget.State.Revision);

                        Assert.Equal(
                            2,
                            ledger.Count);

                        var costEntry =
                            Assert.IsType<ResourceCostLedgerEntry>(
                                ledger.Entries[0]);

                        Assert.Equal(
                            30d,
                            costEntry.Result.ActualCost);

                        var lossEntry =
                            Assert.IsType<ResourceLossLedgerEntry>(
                                ledger.Entries[1]);

                        Assert.Equal(
                            40d,
                            lossEntry.Result.ActualLoss);

                        Assert.Equal(
                            DefeatAwareResourceTransactionCommitOutcome
                                .NoDefeatTransition,
                            domainEvent.Outcome);

                        damageReaction.Dispatch(
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

                        Assert.Equal(
                            70d,
                            setup.ManaTarget.State.Current);

                        Assert.Equal(
                            60d,
                            setup.LifeTarget.State.Current);
                    });
            });

        var session =
            new SimulationSession<ISimulationWorkItem>(
                builder.Build(),
                new SimulationSchedulerLimits(
                    maxQueueSize: 100,
                    maxSameTimestampWave: 10),
                new SimulationRunnerLimits(
                    maxProcessedEvents: 20UL));

        session.ScheduleExternalInput(
            new SimulationTime(200L),
            new PayAttackCostAction());

        var result =
            session.RunToCompletion();

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.Equal(
            6UL,
            session.ProcessedEvents);

        Assert.Equal(
            new[]
            {
            nameof(PayAttackCostAction),
            nameof(AttackCostCommittedEvent),
            nameof(ResolvePaidAttackDamageAction),
            nameof(ApplyResolvedDamageAction),
            nameof(DamageCommittedEvent),
            nameof(ObserveCommittedDamageAction)
            },
            trace);

        Assert.Equal(
            new uint[]
            {
            0u,
            1u,
            2u,
            3u,
            4u,
            5u
            },
            waves);

        Assert.Equal(
            70d,
            setup.ManaTarget.State.Current);

        Assert.Equal(
            60d,
            setup.LifeTarget.State.Current);
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

    private sealed class ObserveCommittedDamageAction
        : ISimulationWorkItem
    {
    }

    private sealed class PayAttackCostAction
    : ISimulationWorkItem
    {
    }

    private sealed class AttackCostCommittedEvent
        : IDomainEvent
    {
    }

    private sealed class ResolvePaidAttackDamageAction
        : ISimulationWorkItem
    {
    }
    private sealed class ResolvePaidAttackDamageReaction
    : IDomainReaction<
        AttackCostCommittedEvent,
        ISimulationWorkItem>
    {
        public void React(
            AttackCostCommittedEvent domainEvent,
            DomainReactionContext<ISimulationWorkItem> context)
        {
            ArgumentNullException.ThrowIfNull(
                domainEvent);

            ArgumentNullException.ThrowIfNull(
                context);

            context.ScheduleFollowUp(
                new ResolvePaidAttackDamageAction());
        }
    }
    private static PaidAttackSetup
    CreatePaidAttackSetup()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.mana"),
                ResourceRole.CostSource),

            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant)
            ]);

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var attacker =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    manaId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var target =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 100d,
                    maximum: 100d)
                ]);

        return new PaidAttackSetup(
            registry,
            manaId,
            lifeId,
            attacker,
            target,
            new ResourceStateTarget(
                attacker,
                manaId),
            new ResourceStateTarget(
                target,
                lifeId));
    }
    private sealed record PaidAttackSetup(
    CompiledResourceRegistry Registry,
    ResourceId ManaId,
    ResourceId LifeId,
    EntityRuntimeState Attacker,
    EntityRuntimeState Target,
    ResourceStateTarget ManaTarget,
    ResourceStateTarget LifeTarget);


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