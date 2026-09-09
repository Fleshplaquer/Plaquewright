using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Combat;

public sealed class ProjectedEntityDefeatEvaluatorTests
{
    [Fact]
    public void AnyPolicy_OneProjectedDepletion_IsDefeated()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageFullLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        var evaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.False(
            evaluation.WasOriginallyDefeated);

        Assert.True(
            evaluation.IsProjectedDefeated);

        Assert.True(
            evaluation.IsNewDefeatTransition);
    }

    [Fact]
    public void AllPolicy_OneOfTwoProjectedDepleted_IsNotDefeated()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageFullLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        var evaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                DefeatRelevantResourcePolicy.AllDepleted);

        Assert.False(
            evaluation.WasOriginallyDefeated);

        Assert.False(
            evaluation.IsProjectedDefeated);

        Assert.False(
            evaluation.IsNewDefeatTransition);
    }

    [Fact]
    public void AllPolicy_AllProjectedDepleted_IsNewDefeatTransition()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageFullLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        StageFullLoss(
            draft,
            setup.SoulTarget,
            setup.SoulId,
            amount: 50d);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        var evaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                DefeatRelevantResourcePolicy.AllDepleted);

        Assert.False(
            evaluation.WasOriginallyDefeated);

        Assert.True(
            evaluation.IsProjectedDefeated);

        Assert.True(
            evaluation.IsNewDefeatTransition);
    }

    [Fact]
    public void AllPolicy_PreviouslyDepletedResourcePlusNewDepletion_CreatesTransition()
    {
        var setup =
            CreateSetup(
                currentLife: 0d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageFullLoss(
            draft,
            setup.SoulTarget,
            setup.SoulId,
            amount: 50d);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        var evaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                DefeatRelevantResourcePolicy.AllDepleted);

        Assert.False(
            evaluation.WasOriginallyDefeated);

        Assert.True(
            evaluation.IsProjectedDefeated);

        Assert.True(
            evaluation.IsNewDefeatTransition);
    }

    [Fact]
    public void AnyPolicy_AlreadyDepletedResource_IsNotNewDefeatTransition()
    {
        var setup =
            CreateSetup(
                currentLife: 0d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        var evaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.True(
            evaluation.WasOriginallyDefeated);

        Assert.True(
            evaluation.IsProjectedDefeated);

        Assert.False(
            evaluation.IsNewDefeatTransition);
    }

    [Fact]
    public void AllPolicy_AllAlreadyDepleted_IsNotNewDefeatTransition()
    {
        var setup =
            CreateSetup(
                currentLife: 0d,
                currentSoul: 0d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        var evaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                DefeatRelevantResourcePolicy.AllDepleted);

        Assert.True(
            evaluation.WasOriginallyDefeated);

        Assert.True(
            evaluation.IsProjectedDefeated);

        Assert.False(
            evaluation.IsNewDefeatTransition);
    }

    [Fact]
    public void SameObservation_CanProduceDifferentResultUnderDifferentPolicies()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        StageFullLoss(
            draft,
            setup.LifeTarget,
            setup.LifeId,
            amount: 100d);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        var anyEvaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                DefeatRelevantResourcePolicy.AnyDepleted);

        var allEvaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                DefeatRelevantResourcePolicy.AllDepleted);

        Assert.True(
            anyEvaluation.IsProjectedDefeated);

        Assert.False(
            allEvaluation.IsProjectedDefeated);

        Assert.Same(
            observation,
            anyEvaluation.Observation);

        Assert.Same(
            observation,
            allEvaluation.Observation);
    }

    [Fact]
    public void ProjectedRecoveryCanMakePreviouslyDefeatedAnyPolicyNotProjectedDefeated()
    {
        var setup =
            CreateSetup(
                currentLife: 0d,
                currentSoul: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        draft.StageRecovery(
            setup.LifeTarget,
            new ResourceRecoveryRequest(
                setup.LifeId,
                amount: 25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        var evaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                DefeatRelevantResourcePolicy.AnyDepleted);

        Assert.True(
            evaluation.WasOriginallyDefeated);

        Assert.False(
            evaluation.IsProjectedDefeated);

        Assert.False(
            evaluation.IsNewDefeatTransition);
    }

    [Fact]
    public void NoDefeatRelevantResources_IsNotDefeatedUnderEitherPrimitivePolicy()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.CostSource)
            ]);

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        manaId,
                        current: 0d,
                        maximum: 100d)
                ]);

        var draft =
            new ResourceTransactionDraft(
                registry);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                entity);

        Assert.Equal(
            0,
            observation.DefeatRelevantResourceCount);

        var anyEvaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                DefeatRelevantResourcePolicy.AnyDepleted);

        var allEvaluation =
            ProjectedEntityDefeatEvaluator.Evaluate(
                observation,
                DefeatRelevantResourcePolicy.AllDepleted);

        Assert.False(
            anyEvaluation.WasOriginallyDefeated);

        Assert.False(
            anyEvaluation.IsProjectedDefeated);

        Assert.False(
            allEvaluation.WasOriginallyDefeated);

        Assert.False(
            allEvaluation.IsProjectedDefeated);
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

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        var invalidPolicy =
            (DefeatRelevantResourcePolicy)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                ProjectedEntityDefeatEvaluator.Evaluate(
                    observation,
                    invalidPolicy));
    }

    private static void StageFullLoss(
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