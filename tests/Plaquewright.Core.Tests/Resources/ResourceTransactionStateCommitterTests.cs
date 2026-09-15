using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceTransactionStateCommitterTests
{
    [Fact]
    public void Commit_EmptyDraft_IsNoOp()
    {
        var draft =
            new ResourceTransactionDraft(
                CreateRegistry());

        ResourceTransactionStateCommitter.Commit(
            draft);

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);
    }

    [Fact]
    public void Commit_AppliesProjectedStateToOriginal()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            CreateLifeEntity(
                registry,
                entityId: 1UL,
                current: 100d,
                maximum: 100d);

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

        Assert.Equal(
            100d,
            state.Current);

        Assert.Equal(
            0UL,
            state.Revision);

        ResourceTransactionStateCommitter.Commit(
            draft);

        Assert.Equal(
            70d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);
    }

    [Fact]
    public void MultipleOperationsOnSameResource_CommitFinalStateOnce()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            CreateLifeEntity(
                registry,
                entityId: 1UL,
                current: 100d,
                maximum: 100d);

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

        draft.StageLoss(
            target,
            CreateLossRequest(
                lifeId,
                20d));

        draft.StageRecovery(
            target,
            new ResourceRecoveryRequest(
                lifeId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        Assert.Equal(
            60d,
            draft.Projections[0].Current);

        ResourceTransactionStateCommitter.Commit(
            draft);

        Assert.Equal(
            60d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);
    }

    [Fact]
    public void Commit_AppliesMultipleResources()
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
                entityId: 1UL,
                lifeCurrent: 100d,
                lifeMaximum: 100d,
                manaCurrent: 50d,
                manaMaximum: 50d);

        var lifeTarget =
            new ResourceStateTarget(
                entity,
                lifeId);

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        var life =
            lifeTarget.State;

        var mana =
            manaTarget.State;

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            lifeTarget,
            CreateLossRequest(
                lifeId,
                25d));

        draft.StageCost(
            manaTarget,
            new ResourceCostRequest(
                manaId,
                20d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost)));

        ResourceTransactionStateCommitter.Commit(
            draft);

        Assert.Equal(
            75d,
            life.Current);

        Assert.Equal(
            30d,
            mana.Current);

        Assert.Equal(
            1UL,
            life.Revision);

        Assert.Equal(
            1UL,
            mana.Revision);
    }

    [Fact]
    public void StaleLaterProjection_PreventsAllTransactionMutations()
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
                entityId: 1UL,
                lifeCurrent: 100d,
                lifeMaximum: 100d,
                manaCurrent: 50d,
                manaMaximum: 50d);

        var lifeTarget =
            new ResourceStateTarget(
                entity,
                lifeId);

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        var life =
            lifeTarget.State;

        var mana =
            manaTarget.State;

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            lifeTarget,
            CreateLossRequest(
                lifeId,
                25d));

        draft.StageCost(
            manaTarget,
            new ResourceCostRequest(
                manaId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost)));

        mana.SetValues(
            current: 40d,
            maximum: 50d);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceTransactionStateCommitter.Commit(
                    draft));

        Assert.Equal(
            100d,
            life.Current);

        Assert.Equal(
            0UL,
            life.Revision);

        Assert.Equal(
            40d,
            mana.Current);

        Assert.Equal(
            1UL,
            mana.Revision);
    }

    [Fact]
    public void Commit_PreservesOperationCountWhileApplyingStateOnce()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            CreateLifeEntity(
                registry,
                entityId: 1UL,
                current: 100d,
                maximum: 100d);

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
                10d));

        draft.StageLoss(
            target,
            CreateLossRequest(
                lifeId,
                15d));

        draft.StageRecovery(
            target,
            new ResourceRecoveryRequest(
                lifeId,
                5d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery)));

        Assert.Equal(
            3,
            draft.OperationCount);

        Assert.Equal(
            1,
            draft.ProjectedResourceCount);

        ResourceTransactionStateCommitter.Commit(
            draft);

        Assert.Equal(
            80d,
            state.Current);

        Assert.Equal(
            1UL,
            state.Revision);

        Assert.Equal(
            3,
            draft.OperationCount);
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