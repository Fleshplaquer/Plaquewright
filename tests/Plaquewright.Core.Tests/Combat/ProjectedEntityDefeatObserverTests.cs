using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProjectedEntityDefeatObserverTests
{
    [Fact]
    public void Observe_IncludesOnlyDefeatRelevantResources()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        Assert.Equal(
            setup.Entity.Id,
            observation.EntityId);

        Assert.Equal(
            2,
            observation.DefeatRelevantResourceCount);

        Assert.Equal(
            2,
            observation.Resources.Count);

        Assert.Contains(
            observation.Resources,
            resource =>
                resource.ResourceId ==
                setup.LifeId);

        Assert.Contains(
            observation.Resources,
            resource =>
                resource.ResourceId ==
                setup.SoulId);

        Assert.DoesNotContain(
            observation.Resources,
            resource =>
                resource.ResourceId ==
                setup.ManaId);
    }

    [Fact]
    public void Observe_ReportsProjectedDepletionWithoutDeclaringDefeat()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        draft.StageLoss(
            setup.LifeTarget,
            new ResourceLossRequest(
                setup.LifeId,
                amount: 100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        Assert.Equal(
            2,
            observation.DefeatRelevantResourceCount);

        Assert.Equal(
            1,
            observation.ProjectedDepletedResourceCount);

        Assert.Equal(
            1,
            observation.NewDepletionCandidateCount);

        Assert.True(
            observation.HasAnyProjectedDepletion);

        Assert.True(
            observation.HasAnyNewDepletionCandidate);

        var life =
            Assert.Single(
                observation.Resources,
                resource =>
                    resource.ResourceId ==
                    setup.LifeId);

        Assert.Equal(
            100d,
            life.OriginalCurrent);

        Assert.Equal(
            0d,
            life.ProjectedCurrent);

        Assert.True(
            life.IsProjected);

        Assert.True(
            life.IsProjectedDepleted);

        Assert.True(
            life.IsNewDepletionCandidate);

        var soul =
            Assert.Single(
                observation.Resources,
                resource =>
                    resource.ResourceId ==
                    setup.SoulId);

        Assert.Equal(
            50d,
            soul.ProjectedCurrent);

        Assert.False(
            soul.IsProjected);

        Assert.False(
            soul.IsProjectedDepleted);

        Assert.False(
            soul.IsNewDepletionCandidate);
    }

    [Fact]
    public void AlreadyDepletedResource_IsObservedButNotNewCandidate()
    {
        var setup =
            CreateSetup(
                currentLife: 0d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        Assert.Equal(
            1,
            observation.ProjectedDepletedResourceCount);

        Assert.Equal(
            0,
            observation.NewDepletionCandidateCount);

        Assert.True(
            observation.HasAnyProjectedDepletion);

        Assert.False(
            observation.HasAnyNewDepletionCandidate);

        var life =
            Assert.Single(
                observation.Resources,
                resource =>
                    resource.ResourceId ==
                    setup.LifeId);

        Assert.Equal(
            0d,
            life.OriginalCurrent);

        Assert.Equal(
            0d,
            life.ProjectedCurrent);

        Assert.False(
            life.IsNewDepletionCandidate);
    }

    [Fact]
    public void MultipleResourcesCanBecomeNewCandidatesInSameTransaction()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        draft.StageLoss(
            setup.LifeTarget,
            new ResourceLossRequest(
                setup.LifeId,
                amount: 100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        draft.StageLoss(
            setup.SoulTarget,
            new ResourceLossRequest(
                setup.SoulId,
                amount: 50d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Sacrifice)));

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        Assert.Equal(
            2,
            observation.ProjectedDepletedResourceCount);

        Assert.Equal(
            2,
            observation.NewDepletionCandidateCount);

        Assert.True(
            observation.HasAnyProjectedDepletion);

        Assert.True(
            observation.HasAnyNewDepletionCandidate);

        Assert.All(
            observation.Resources,
            resource =>
                Assert.True(
                    resource.IsProjectedDepleted));

        Assert.All(
            observation.Resources,
            resource =>
                Assert.True(
                    resource.IsNewDepletionCandidate));
    }

    [Fact]
    public void RecoveryBeforeObservationCanRemoveCandidate()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        draft.StageLoss(
            setup.LifeTarget,
            new ResourceLossRequest(
                setup.LifeId,
                amount: 100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

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

        Assert.Equal(
            0,
            observation.ProjectedDepletedResourceCount);

        Assert.Equal(
            0,
            observation.NewDepletionCandidateCount);

        Assert.False(
            observation.HasAnyProjectedDepletion);

        Assert.False(
            observation.HasAnyNewDepletionCandidate);

        var life =
            Assert.Single(
                observation.Resources,
                resource =>
                    resource.ResourceId ==
                    setup.LifeId);

        Assert.Equal(
            25d,
            life.ProjectedCurrent);

        Assert.False(
            life.IsProjectedDepleted);

        Assert.False(
            life.IsNewDepletionCandidate);
    }

    [Fact]
    public void NonDefeatRelevantResourceDepletion_DoesNotAffectObservation()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        draft.StageLoss(
            setup.ManaTarget,
            new ResourceLossRequest(
                setup.ManaId,
                amount: 40d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.ProtectionFinancing)));

        var observation =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        Assert.Equal(
            2,
            observation.DefeatRelevantResourceCount);

        Assert.Equal(
            0,
            observation.ProjectedDepletedResourceCount);

        Assert.Equal(
            0,
            observation.NewDepletionCandidateCount);

        Assert.False(
            observation.HasAnyProjectedDepletion);

        Assert.False(
            observation.HasAnyNewDepletionCandidate);
    }

    [Fact]
    public void Observe_DoesNotMutateDraftOrEntityResources()
    {
        var setup =
            CreateSetup(
                currentLife: 100d,
                currentSoul: 50d,
                currentMana: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        draft.StageLoss(
            setup.LifeTarget,
            new ResourceLossRequest(
                setup.LifeId,
                amount: 100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        var projectionCountBefore =
            draft.ProjectedResourceCount;

        var operationCountBefore =
            draft.OperationCount;

        _ =
            ProjectedEntityDefeatObserver.Observe(
                draft,
                setup.Entity);

        Assert.Equal(
            projectionCountBefore,
            draft.ProjectedResourceCount);

        Assert.Equal(
            operationCountBefore,
            draft.OperationCount);

        Assert.Equal(
            100d,
            setup.LifeTarget.State.Current);

        Assert.Equal(
            50d,
            setup.SoulTarget.State.Current);

        Assert.Equal(
            40d,
            setup.ManaTarget.State.Current);

        Assert.Equal(
            0UL,
            setup.LifeTarget.State.Revision);

        Assert.Equal(
            0UL,
            setup.SoulTarget.State.Revision);

        Assert.Equal(
            0UL,
            setup.ManaTarget.State.Revision);
    }

    private static TestSetup CreateSetup(
        double currentLife,
        double currentSoul,
        double currentMana)
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
                    ResourceRole.DefeatRelevant),

                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.CostSource |
                    ResourceRole.ProtectionSource)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var soulId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.soul"));

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
                        lifeId,
                        currentLife,
                        maximum: 100d),

                    new ResourceState(
                        soulId,
                        currentSoul,
                        maximum: 100d),

                    new ResourceState(
                        manaId,
                        currentMana,
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

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        return new TestSetup(
            registry,
            lifeId,
            soulId,
            manaId,
            entity,
            lifeTarget,
            soulTarget,
            manaTarget);
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId LifeId,
        ResourceId SoulId,
        ResourceId ManaId,
        EntityRuntimeState Entity,
        ResourceStateTarget LifeTarget,
        ResourceStateTarget SoulTarget,
        ResourceStateTarget ManaTarget);
}