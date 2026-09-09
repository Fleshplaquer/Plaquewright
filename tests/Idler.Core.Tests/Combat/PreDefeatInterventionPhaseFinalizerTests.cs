using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Combat;

public sealed class PreDefeatInterventionPhaseFinalizerTests
{
    [Fact]
    public void SuccessfulRecovery_FinalizesAsResolved()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var intervention =
            PreDefeatRecoveryIntervention.Apply(
                context,
                setup.LifeId,
                recoveryAmount: 25d);

        Assert.True(
            intervention.WasDefeatResolved);

        var result =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Resolved,
            result.Outcome);

        Assert.True(
            result.WasResolved);

        Assert.False(
            result.RemainsProjectedDefeated);

        Assert.False(
            result.FinalEvaluation.IsProjectedDefeated);

        Assert.Same(
            context,
            result.Context);
    }

    [Fact]
    public void NoIntervention_FinalizesAsUnresolved()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var result =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Unresolved,
            result.Outcome);

        Assert.False(
            result.WasResolved);

        Assert.True(
            result.RemainsProjectedDefeated);

        Assert.True(
            result.FinalEvaluation.IsProjectedDefeated);

        Assert.True(
            result.FinalEvaluation.IsNewDefeatTransition);
    }

    [Fact]
    public void PartialRecoveryUnderAnyPolicy_CanRemainUnresolved()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        StageLoss(
            draft,
            setup.SoulTarget,
            setup.SoulId,
            amount: 50d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var intervention =
            PreDefeatRecoveryIntervention.Apply(
                context,
                setup.LifeId,
                recoveryAmount: 25d);

        Assert.True(
            intervention.IsStillProjectedDefeated);

        var result =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Unresolved,
            result.Outcome);

        Assert.True(
            result.RemainsProjectedDefeated);

        var life =
            Assert.Single(
                result.FinalEvaluation.Observation.Resources,
                resource =>
                    resource.ResourceId ==
                    setup.LifeId);

        var soul =
            Assert.Single(
                result.FinalEvaluation.Observation.Resources,
                resource =>
                    resource.ResourceId ==
                    setup.SoulId);

        Assert.Equal(
            25d,
            life.ProjectedCurrent);

        Assert.Equal(
            0d,
            soul.ProjectedCurrent);
    }

    [Fact]
    public void RestoringOneResourceUnderAllPolicy_CanResolvePhase()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        StageLoss(
            draft,
            setup.SoulTarget,
            setup.SoulId,
            amount: 50d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AllDepleted);

        Assert.NotNull(
            context);

        PreDefeatRecoveryIntervention.Apply(
            context,
            setup.LifeId,
            recoveryAmount: 1d);

        var result =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Resolved,
            result.Outcome);

        Assert.False(
            result.FinalEvaluation.IsProjectedDefeated);

        Assert.True(
            result.WasResolved);
    }

    [Fact]
    public void TriggerEvaluation_RemainsHistoricalWhenPhaseResolves()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        Assert.True(
            context.TriggerEvaluation.IsProjectedDefeated);

        Assert.True(
            context.TriggerEvaluation.IsNewDefeatTransition);

        PreDefeatRecoveryIntervention.Apply(
            context,
            setup.LifeId,
            recoveryAmount: 25d);

        var result =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        // Why the phase started.
        Assert.True(
            context.TriggerEvaluation.IsProjectedDefeated);

        Assert.True(
            context.TriggerEvaluation.IsNewDefeatTransition);

        // How the phase ended.
        Assert.False(
            result.FinalEvaluation.IsProjectedDefeated);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Resolved,
            result.Outcome);
    }

    [Fact]
    public void PhaseResult_IsSnapshotOfStateAtFinalizationTime()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var firstResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Unresolved,
            firstResult.Outcome);

        // Draft changes after the first phase snapshot.
        draft.StageRecovery(
            setup.LifeTarget,
            new ResourceRecoveryRequest(
                setup.LifeId,
                amount: 25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        var secondResult =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            PreDefeatInterventionPhaseOutcome.Resolved,
            secondResult.Outcome);

        // Previous result remains a snapshot.
        Assert.True(
            firstResult.FinalEvaluation.IsProjectedDefeated);

        Assert.False(
            secondResult.FinalEvaluation.IsProjectedDefeated);
    }

    [Fact]
    public void Finalization_DoesNotMutateDraftOrEntityState()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
            context);

        var operationCountBefore =
            draft.OperationCount;

        var projectionCountBefore =
            draft.ProjectedResourceCount;

        _ =
            PreDefeatInterventionPhaseFinalizer.Finalize(
                context);

        Assert.Equal(
            operationCountBefore,
            draft.OperationCount);

        Assert.Equal(
            projectionCountBefore,
            draft.ProjectedResourceCount);

        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    private static void StageLoss(
        ResourceTransactionDraft draft,
        ResourceStateTarget target,
        ResourceId resourceId,
        double amount)
    {
        draft.StageLoss(
            target,
            new ResourceLossRequest(
                resourceId,
                amount,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));
    }

    private static TestSetup CreateSetup(
        double currentLife,
        double currentSoul)
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.life"),
                    ResourceRole.DamageTarget |
                    ResourceRole.DefeatRelevant),

                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.soul"),
                    ResourceRole.DefeatRelevant)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var soulId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.soul"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        currentLife,
                        maximum: 100d),

                    new ResourceState(
                        soulId,
                        currentSoul,
                        maximum: 100d)
                ]);

        return new TestSetup(
            registry,
            lifeId,
            soulId,
            entity,
            new ResourceStateTarget(
                entity,
                lifeId),
            new ResourceStateTarget(
                entity,
                soulId));
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId LifeId,
        ResourceId SoulId,
        EntityRuntimeState Entity,
        ResourceStateTarget LifeTarget,
        ResourceStateTarget SoulTarget);
}