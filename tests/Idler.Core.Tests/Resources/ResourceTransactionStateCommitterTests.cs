using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

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

        var state =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            state,
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

        var state =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            state,
            CreateLossRequest(
                lifeId,
                30d));

        draft.StageLoss(
            state,
            CreateLossRequest(
                lifeId,
                20d));

        draft.StageRecovery(
            state,
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

        var life =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var mana =
            new ResourceState(
                manaId,
                current: 50d,
                maximum: 50d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            life,
            CreateLossRequest(
                lifeId,
                25d));

        draft.StageCost(
            mana,
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

        var life =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var mana =
            new ResourceState(
                manaId,
                current: 50d,
                maximum: 50d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        // Life is first-touch / first projection.
        draft.StageLoss(
            life,
            CreateLossRequest(
                lifeId,
                25d));

        // Mana is deliberately the later projection.
        draft.StageCost(
            mana,
            new ResourceCostRequest(
                manaId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost)));

        // External mutation makes only the later
        // projection stale.
        mana.SetValues(
            current: 40d,
            maximum: 50d);

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceTransactionStateCommitter.Commit(
                    draft));

        // Critical atomicity assertion:
        // Life passed its own preflight, but must
        // still not have been mutated.
        Assert.Equal(
            100d,
            life.Current);

        Assert.Equal(
            0UL,
            life.Revision);

        // Only the explicit external mutation exists.
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

        var state =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            state,
            CreateLossRequest(
                lifeId,
                10d));

        draft.StageLoss(
            state,
            CreateLossRequest(
                lifeId,
                15d));

        draft.StageRecovery(
            state,
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