using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Combat;

public sealed class PreDefeatInterventionContextFactoryTests
{
    [Fact]
    public void NewAnyPolicyDefeatTransition_CreatesContext()
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

        Assert.Same(
            draft,
            context.Draft);

        Assert.Same(
            setup.Entity,
            context.Entity);

        Assert.Equal(
            setup.Entity.Id,
            context.EntityId);

        Assert.Equal(
            DefeatRelevantResourcePolicy.AnyDepleted,
            context.Policy);

        Assert.False(
            context.TriggerEvaluation.WasOriginallyDefeated);

        Assert.True(
            context.TriggerEvaluation.IsProjectedDefeated);

        Assert.True(
            context.TriggerEvaluation.IsNewDefeatTransition);

        Assert.Same(
            context.TriggerEvaluation.Observation,
            context.TriggerObservation);
    }

    [Fact]
    public void NoProjectedDefeatTransition_ReturnsNull()
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
            amount: 50d);

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.Null(
            context);

        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);
    }

    [Fact]
    public void AllPolicy_RequiresAllRelevantResourcesToBecomeDepleted()
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
                DefeatRelevantResourcePolicy.AllDepleted);

        Assert.Null(
            context);

        StageLoss(
            draft,
            setup.SoulTarget,
            setup.SoulId,
            amount: 50d);

        context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AllDepleted);

        Assert.NotNull(
            context);

        Assert.True(
            context.TriggerEvaluation.IsNewDefeatTransition);

        Assert.Equal(
            2,
            context.TriggerObservation.ProjectedDepletedResourceCount);
    }

    [Fact]
    public void AlreadyDefeatedEntity_DoesNotCreateNewContext()
    {
        var setup =
            CreateSetup(
                currentLife: 0d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

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

        Assert.Null(
            context);
    }

    [Fact]
    public void PreviouslyPartiallyDepletedEntity_CanTriggerAllPolicy()
    {
        var setup =
            CreateSetup(
                currentLife: 0d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

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

        Assert.False(
            context.TriggerEvaluation.WasOriginallyDefeated);

        Assert.True(
            context.TriggerEvaluation.IsProjectedDefeated);

        Assert.True(
            context.TriggerEvaluation.IsNewDefeatTransition);
    }

    [Fact]
    public void CreatingContext_DoesNotCommitProjectedResourceChanges()
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

        var operationCountBefore =
            draft.OperationCount;

        var projectionCountBefore =
            draft.ProjectedResourceCount;

        var context =
            PreDefeatInterventionContextFactory.TryCreate(
                draft,
                setup.Entity,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.NotNull(
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

    [Fact]
    public void TriggerEvaluation_RemainsTheReasonContextWasCreated()
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
            context.TriggerEvaluation.IsNewDefeatTransition);

        // Simulate a later intervention changing the same draft.
        draft.StageRecovery(
            setup.LifeTarget,
            new ResourceRecoveryRequest(
                setup.LifeId,
                amount: 25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        var currentObservation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        var currentEvaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                currentObservation,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.False(
            currentEvaluation.IsProjectedDefeated);

        Assert.False(
            currentEvaluation.IsNewDefeatTransition);

        // The trigger snapshot still records why
        // pre-defeat processing originally started.
        Assert.True(
            context.TriggerEvaluation.IsProjectedDefeated);

        Assert.True(
            context.TriggerEvaluation.IsNewDefeatTransition);

        Assert.Same(
            draft,
            context.Draft);
    }

    [Fact]
    public void DifferentRegistry_IsRejectedBeforeContextCreation()
    {
        var first =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var second =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                first.Registry);

        StageLoss(
            draft,
            first.LifeTarget,
            first.LifeId,
            amount: 100d);

        Assert.Throws<ArgumentException>(
            () =>
                PreDefeatInterventionContextFactory.TryCreate(
                    draft,
                    second.Entity,
                    DefeatRelevantResourcePolicy.AnyDepleted));

        Assert.Equal(
            100d,
            second.LifeTarget.State.Current);

        Assert.Equal(
            0UL,
            second.LifeTarget.State.Revision);
    }

    [Fact]
    public void UnknownPolicy_Throws()
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

        var invalidPolicy =
            (DefeatRelevantResourcePolicy)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                PreDefeatInterventionContextFactory.TryCreate(
                    draft,
                    setup.Entity,
                    invalidPolicy));

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

        var lifeTarget =
            new ResourceStateTarget(
                entity,
                lifeId);

        var soulTarget =
            new ResourceStateTarget(
                entity,
                soulId);

        return new TestSetup(
            registry,
            lifeId,
            soulId,
            entity,
            lifeTarget,
            soulTarget);
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId LifeId,
        ResourceId SoulId,
        EntityRuntimeState Entity,
        ResourceStateTarget LifeTarget,
        ResourceStateTarget SoulTarget);
}