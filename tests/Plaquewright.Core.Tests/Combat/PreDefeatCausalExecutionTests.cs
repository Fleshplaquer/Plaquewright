using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class PreDefeatCausalExecutionTests
{
    [Theory]
    [InlineData(50d)]
    [InlineData(100d)]
    public void InvalidGameplayExecution_ThrowsWithOrWithoutNewDefeatTransition(
        double damageLoss)
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageDamageLoss(
            draft,
            setup.LifeTarget,
            damageLoss,
            new ExecutionId(11UL));

        var versionBefore =
            draft.Version;

        var operationCountBefore =
            draft.OperationCount;

        var projectionCountBefore =
            draft.ProjectedResourceCount;

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    PreDefeatInterventionContextFactory.TryCreate(
                        draft,
                        setup.Entity,
                        DefeatRelevantResourcePolicy.AnyDepleted,
                        default(ExecutionId)));

        Assert.Equal(
            "gameplayExecutionId",
            exception.ParamName);

        Assert.Equal(
            versionBefore,
            draft.Version);

        Assert.Equal(
            operationCountBefore,
            draft.OperationCount);

        Assert.Equal(
            projectionCountBefore,
            draft.ProjectedResourceCount);

        Assert.Equal(
            100d - damageLoss,
            draft.GetProjectedValues(
                setup.LifeTarget).Current);

        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    [Fact]
    public void ContextConstructor_RejectsInvalidGameplayExecution()
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageDamageLoss(
            draft,
            setup.LifeTarget,
            amount: 100d,
            new ExecutionId(11UL));

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var versionBefore =
            draft.Version;

        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new PreDefeatInterventionContext(
                        draft,
                        setup.Entity,
                        context.TriggerEvaluation,
                        default(ExecutionId)));

        Assert.Equal(
            "gameplayExecutionId",
            exception.ParamName);

        Assert.Equal(
            versionBefore,
            draft.Version);

        Assert.Single(
            draft.Operations);
    }

    [Fact]
    public void ValidGameplayExecution_WithoutNewDefeatTransition_ReturnsNull()
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageDamageLoss(
            draft,
            setup.LifeTarget,
            amount: 50d,
            new ExecutionId(11UL));

        var versionBefore =
            draft.Version;

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted,
                new ExecutionId(12UL));

        Assert.Null(
            context);

        Assert.Equal(
            versionBefore,
            draft.Version);

        Assert.Single(
            draft.Operations);

        Assert.Equal(
            50d,
            draft.GetProjectedValues(
                setup.LifeTarget).Current);

        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ContextFreeIntervention_DoesNotInferExecutionFromDamage(
        bool useMinimumCurrent)
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var damageExecutionId =
            new ExecutionId(11UL);

        StageDamageLoss(
            draft,
            setup.LifeTarget,
            amount: 100d,
            damageExecutionId);

        // The compatibility overload must remain deliberately context-free,
        // even though the draft contains a loss with an execution ID.
        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        Assert.False(
            context.HasGameplayExecution);

        Assert.Null(
            context.GameplayExecutionId);

        ApplyIntervention(
            context,
            setup.LifeId,
            useMinimumCurrent,
            amount: 25d);

        Assert.Equal(
            2,
            draft.OperationCount);

        var stagedRecovery =
            Assert.IsType<StagedResourceRecoveryOperation>(
                draft.Operations[1]);

        var expectedRecoveryProvenance =
            new ResourceOperationProvenance(
                ResourceOperationCause.Recovery);

        Assert.Equal(
            expectedRecoveryProvenance,
            stagedRecovery.Provenance);

        var ledger =
            CommitResolved(
                context);

        Assert.Equal(
            2,
            ledger.Count);

        var lossEntry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            damageExecutionId,
            lossEntry.Provenance.GameplayExecutionId);

        var recoveryEntry =
            Assert.IsType<ResourceRecoveryLedgerEntry>(
                ledger.Entries[1]);

        Assert.Equal(
            expectedRecoveryProvenance,
            recoveryEntry.Provenance);

        Assert.False(
            recoveryEntry.Provenance.HasGameplayExecution);

        Assert.Equal(
            25d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.LifeTarget.State.Revision);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RepeatedInterventionsInOneDraft_KeepTheirOwnExecutionIds(
        bool useMinimumCurrent)
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var firstDamageId =
            new ExecutionId(11UL);

        var firstInterventionId =
            new ExecutionId(12UL);

        var secondDamageId =
            new ExecutionId(13UL);

        var secondInterventionId =
            new ExecutionId(14UL);

        StageDamageLoss(
            draft,
            setup.LifeTarget,
            amount: 100d,
            firstDamageId);

        var firstContext =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted,
                firstInterventionId);

        Assert.NotNull(
            firstContext);

        ApplyIntervention(
            firstContext,
            setup.LifeId,
            useMinimumCurrent,
            amount: 25d);

        StageDamageLoss(
            draft,
            setup.LifeTarget,
            amount: 25d,
            secondDamageId);

        var secondContext =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted,
                secondInterventionId);

        Assert.NotNull(
            secondContext);

        ApplyIntervention(
            secondContext,
            setup.LifeId,
            useMinimumCurrent,
            amount: 10d);

        Assert.Equal(
            firstInterventionId,
            firstContext.GameplayExecutionId);

        Assert.Equal(
            secondInterventionId,
            secondContext.GameplayExecutionId);

        ResourceOperationProvenance[] expectedProvenance =
        [
            new(ResourceOperationCause.DamageDerived, firstDamageId),
            new(ResourceOperationCause.Recovery, firstInterventionId),
            new(ResourceOperationCause.DamageDerived, secondDamageId),
            new(ResourceOperationCause.Recovery, secondInterventionId)
        ];

        Assert.Equal(
            expectedProvenance.Length,
            draft.OperationCount);

        for (var index = 0;
             index < expectedProvenance.Length;
             index++)
        {
            Assert.Equal(
                expectedProvenance[index],
                draft.Operations[index].Provenance);
        }

        // Multiple causal operations still publish only one final resource state.
        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        var ledger =
            CommitResolved(
                secondContext);

        Assert.Equal(
            expectedProvenance.Length,
            ledger.Count);

        for (var index = 0;
             index < expectedProvenance.Length;
             index++)
        {
            Assert.Equal(
                expectedProvenance[index],
                ledger.Entries[index].Provenance);

            Assert.Equal(
                setup.Entity.Id,
                ledger.Entries[index].TargetEntityId);

            Assert.Equal(
                setup.LifeId,
                ledger.Entries[index].ResourceId);
        }

        Assert.Equal(
            10d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            1UL,
            setup.LifeTarget.State.Revision);
    }

    [Fact]
    public void MinimumCurrent_AlreadySatisfied_DoesNotStageExecutionOnlyOperation()
    {
        var setup =
            CreateSetup();

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        // Soul triggers pre-defeat; Life is already above the requested minimum.
        StageDamageLoss(
            draft,
            setup.SoulTarget,
            amount: 50d,
            new ExecutionId(11UL));

        var interventionId =
            new ExecutionId(12UL);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted,
                interventionId);

        Assert.NotNull(
            context);

        Assert.True(
            context.HasGameplayExecution);

        Assert.Equal(
            interventionId,
            context.GameplayExecutionId);

        var versionBefore =
            draft.Version;

        var projectionCountBefore =
            draft.ProjectedResourceCount;

        var result =
            PreDefeatMinimumCurrentIntervention.Apply(
                context,
                setup.LifeId,
                minimumCurrent: 50d);

        Assert.False(
            result.DidStageRecovery);

        Assert.Equal(
            0d,
            result.RequestedRecovery);

        Assert.True(
            result.IsStillProjectedDefeated);

        Assert.Equal(
            versionBefore,
            draft.Version);

        Assert.Equal(
            projectionCountBefore,
            draft.ProjectedResourceCount);

        Assert.Single(
            draft.Operations);

        Assert.Equal(
            100d,
            draft.GetProjectedValues(
                setup.LifeTarget).Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    private static void ApplyIntervention(
        PreDefeatInterventionContext context,
        ResourceId resourceId,
        bool useMinimumCurrent,
        double amount)
    {
        if (useMinimumCurrent)
        {
            var result =
                PreDefeatMinimumCurrentIntervention.Apply(
                    context,
                    resourceId,
                    minimumCurrent: amount);

            Assert.True(
                result.WasDefeatResolved);
        }
        else
        {
            var result =
                PreDefeatRecoveryIntervention.Apply(
                    context,
                    resourceId,
                    recoveryAmount: amount);

            Assert.True(
                result.WasDefeatResolved);
        }
    }

    private static ResourceOperationLedger CommitResolved(
        PreDefeatInterventionContext context)
    {
        var phaseResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Resolved,
            phaseResult.Outcome);

        var ledger =
            new ResourceOperationLedger();

        var commitResult =
            DefeatAwareResourceTransactionCommitter.Commit(
                context.Draft,
                ledger,
                context.Entity,
                context.Policy,
                phaseResult);

        Assert.Equal(
            DefeatAwareResourceTransactionCommitOutcome.DefeatPrevented,
            commitResult.Outcome);

        return ledger;
    }

    private static void StageDamageLoss(
        ResourceTransactionDraft draft,
        ResourceStateTarget target,
        double amount,
        ExecutionId gameplayExecutionId)
    {
        draft.StageLoss(
            target,
            new ResourceLossRequest(
                target.ResourceId,
                amount,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived,
                    gameplayExecutionId)));
    }

    private static TestSetup CreateSetup()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse("resource.life"),
                    ResourceRole.DamageTarget |
                    ResourceRole.DefeatRelevant),

                new ResourceDefinition(
                    ResourceKey.Parse("resource.soul"),
                    ResourceRole.DefeatRelevant)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse("resource.life"));

        var soulId =
            registry.GetId(
                ResourceKey.Parse("resource.soul"));

        var entity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d),

                    new ResourceState(
                        soulId,
                        current: 50d,
                        maximum: 100d)
                ]);

        return new TestSetup(
            registry,
            entity,
            lifeId,
            new ResourceStateTarget(entity, lifeId),
            new ResourceStateTarget(entity, soulId));
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        EntityRuntimeState Entity,
        ResourceId LifeId,
        ResourceStateTarget LifeTarget,
        ResourceStateTarget SoulTarget);
}
