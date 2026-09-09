using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceTransactionDraftTests
{
    [Fact]
    public void NewDraft_IsEmpty()
    {
        var draft =
            new ResourceTransactionDraft(
                CreateRegistry());

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);

        Assert.Empty(
            draft.Projections);

        Assert.Empty(
            draft.Operations);
    }

    [Fact]
    public void SequentialLosses_OnSameState_UseSameProjection()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            CreateLifeEntity(
                registry,
                1UL,
                100d,
                100d);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var state =
            target.State;

        var draft =
            new ResourceTransactionDraft(
                registry);

        var first =
            draft.StageLoss(
                target,
                CreateLossRequest(
                    lifeId,
                    30d));

        var second =
            draft.StageLoss(
                target,
                CreateLossRequest(
                    lifeId,
                    20d));

        Assert.Equal(
            70d,
            first.CurrentAfter);

        Assert.Equal(
            50d,
            second.CurrentAfter);

        Assert.Equal(
            1,
            draft.ProjectedResourceCount);

        Assert.Equal(
            2,
            draft.OperationCount);

        Assert.Equal(
            50d,
            draft.Projections[0].Current);

        Assert.Equal(
            100d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void SameResourceId_OnDifferentStates_UsesSeparateProjections()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var firstEntity =
            CreateLifeEntity(
                registry,
                1UL,
                100d,
                100d);

        var secondEntity =
            CreateLifeEntity(
                registry,
                2UL,
                200d,
                200d);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                lifeId);

        var firstState =
            firstTarget.State;

        var secondState =
            secondTarget.State;

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            firstTarget,
            CreateLossRequest(
                lifeId,
                30d));

        draft.StageLoss(
            secondTarget,
            CreateLossRequest(
                lifeId,
                50d));

        Assert.Equal(
            2,
            draft.ProjectedResourceCount);

        Assert.Same(
            firstState,
            draft.Projections[0].OriginalState);

        Assert.Same(
            secondState,
            draft.Projections[1].OriginalState);

        Assert.Equal(
            70d,
            draft.Projections[0].Current);

        Assert.Equal(
            150d,
            draft.Projections[1].Current);

        Assert.Equal(
            100d,
            firstState.Current);

        Assert.Equal(
            200d,
            secondState.Current);
    }

    [Fact]
    public void Operations_PreserveStageOrderAcrossOperationTypes()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            CreateLifeEntity(
                registry,
                1UL,
                100d,
                100d);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var state =
            target.State;

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            target,
            CreateLossRequest(
                lifeId,
                30d));

        draft.StageRecovery(
            target,
            new ResourceRecoveryRequest(
                lifeId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        draft.StageLoss(
            target,
            CreateLossRequest(
                lifeId,
                5d));

        Assert.Equal(
            3,
            draft.OperationCount);

        Assert.IsType<StagedResourceLossOperation>(
            draft.Operations[0]);

        Assert.IsType<StagedResourceRecoveryOperation>(
            draft.Operations[1]);

        Assert.IsType<StagedResourceLossOperation>(
            draft.Operations[2]);

        Assert.Equal(
            75d,
            draft.Projections[0].Current);

        Assert.Equal(
            100d,
            state.Current);
    }

    [Fact]
    public void UnaffordableCost_IsNotStaged()
    {
        var registry =
            CreateRegistry();

        var manaId =
            GetManaId(
                registry);

        var entity =
            CreateManaEntity(
                registry,
                1UL,
                20d,
                100d);

        var target =
            new ResourceStateTarget(
                entity,
                manaId);

        var state =
            target.State;

        var draft =
            new ResourceTransactionDraft(
                registry);

        Assert.Throws<InvalidOperationException>(
            () =>
                draft.StageCost(
                    target,
                    new ResourceCostRequest(
                        manaId,
                        50d,
                        new ResourceOperationProvenance(
                            ResourceOperationCause.SkillCost))));

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);

        Assert.Equal(
            20d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void FailedLaterCost_DoesNotAlterExistingProjection()
    {
        var registry =
            CreateRegistry();

        var manaId =
            GetManaId(
                registry);

        var entity =
            CreateManaEntity(
                registry,
                1UL,
                50d,
                100d);

        var target =
            new ResourceStateTarget(
                entity,
                manaId);

        var state =
            target.State;

        var draft =
            new ResourceTransactionDraft(
                registry);

        var first =
            draft.StageCost(
                target,
                new ResourceCostRequest(
                    manaId,
                    30d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        Assert.True(
            first.IsAffordable);

        Assert.Equal(
            20d,
            draft.Projections[0].Current);

        Assert.Throws<InvalidOperationException>(
            () =>
                draft.StageCost(
                    target,
                    new ResourceCostRequest(
                        manaId,
                        25d,
                        new ResourceOperationProvenance(
                            ResourceOperationCause.SkillCost))));

        Assert.Equal(
            1,
            draft.ProjectedResourceCount);

        Assert.Equal(
            1,
            draft.OperationCount);

        Assert.Equal(
            20d,
            draft.Projections[0].Current);

        Assert.Equal(
            50d,
            state.Current);
    }

    [Fact]
    public void StagedOperation_PreservesOriginalStateAndProvenance()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            CreateLifeEntity(
                registry,
                42UL,
                100d,
                100d);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var state =
            target.State;

        var provenance =
            new ResourceOperationProvenance(
                ResourceOperationCause.DamageDerived);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            target,
            new ResourceLossRequest(
                lifeId,
                25d,
                provenance));

        var operation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[0]);

        Assert.Same(
            target,
            operation.Target);

        Assert.Equal(
            entity.Id,
            operation.EntityId);

        Assert.Same(
            state,
            operation.OriginalState);

        Assert.Equal(
            lifeId,
            operation.ResourceId);

        Assert.Equal(
            provenance,
            operation.Provenance);

        Assert.Equal(
            25d,
            operation.Preview.Result.ActualLoss);
    }

    [Fact]
    public void ProjectedResources_PreserveFirstTouchOrder()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var manaId =
            GetManaId(
                registry);

        var entity =
            CreateLifeManaEntity(
                registry,
                1UL,
                lifeCurrent: 100d,
                lifeMaximum: 100d,
                manaCurrent: 50d,
                manaMaximum: 50d);

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        var lifeTarget =
            new ResourceStateTarget(
                entity,
                lifeId);

        var mana =
            manaTarget.State;

        var life =
            lifeTarget.State;

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageCost(
            manaTarget,
            new ResourceCostRequest(
                manaId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost)));

        draft.StageLoss(
            lifeTarget,
            CreateLossRequest(
                lifeId,
                20d));

        Assert.Same(
            mana,
            draft.Projections[0].OriginalState);

        Assert.Same(
            life,
            draft.Projections[1].OriginalState);
    }

    private static EntityRuntimeState CreateLifeEntity(
        CompiledResourceRegistry registry,
        ulong entityId,
        double current,
        double maximum)
    {
        return new EntityRuntimeState(
            new EntityId(entityId),
            registry,
            [
                new ResourceState(
                    GetLifeId(registry),
                    current,
                    maximum)
            ]);
    }

    private static EntityRuntimeState CreateManaEntity(
        CompiledResourceRegistry registry,
        ulong entityId,
        double current,
        double maximum)
    {
        return new EntityRuntimeState(
            new EntityId(entityId),
            registry,
            [
                new ResourceState(
                    GetManaId(registry),
                    current,
                    maximum)
            ]);
    }

    private static EntityRuntimeState CreateLifeManaEntity(
        CompiledResourceRegistry registry,
        ulong entityId,
        double lifeCurrent,
        double lifeMaximum,
        double manaCurrent,
        double manaMaximum)
    {
        return new EntityRuntimeState(
            new EntityId(entityId),
            registry,
            [
                new ResourceState(
                    GetLifeId(registry),
                    lifeCurrent,
                    lifeMaximum),

                new ResourceState(
                    GetManaId(registry),
                    manaCurrent,
                    manaMaximum)
            ]);
    }

    private static ResourceLossRequest CreateLossRequest(
        ResourceId resourceId,
        double amount)
    {
        return new ResourceLossRequest(
            resourceId,
            amount,
            new ResourceOperationProvenance(
                ResourceOperationCause.Direct));
    }

    private static ResourceId GetLifeId(
        CompiledResourceRegistry registry)
    {
        return registry.GetId(
            ResourceKey.Parse(
                "resource.life"));
    }

    private static ResourceId GetManaId(
        CompiledResourceRegistry registry)
    {
        return registry.GetId(
            ResourceKey.Parse(
                "resource.mana"));
    }

    private static CompiledResourceRegistry CreateRegistry()
    {
        return ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant),

            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.mana"),
                ResourceRole.CostSource)
        ]);
    }
}