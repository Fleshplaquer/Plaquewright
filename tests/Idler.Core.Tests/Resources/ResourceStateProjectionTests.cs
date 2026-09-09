using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceStateProjectionTests
{
    [Fact]
    public void Constructor_CopiesOriginalStateWithoutMutation()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var original =
            new ResourceState(
                lifeId,
                current: 75d,
                maximum: 100d);

        var projection =
            new ResourceStateProjection(
                registry,
                original);

        Assert.Same(
            original,
            projection.OriginalState);

        Assert.Equal(
            75d,
            projection.Current);

        Assert.Equal(
            100d,
            projection.Maximum);

        Assert.Equal(
            0UL,
            projection.ExpectedOriginalRevision);

        Assert.Equal(
            75d,
            original.Current);

        Assert.Equal(
            0UL,
            original.Revision);
    }

    [Fact]
    public void SequentialLosses_UseProjectedState()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var original =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var projection =
            new ResourceStateProjection(
                registry,
                original);

        var first =
            projection.PreviewLoss(
                new ResourceLossRequest(
                    lifeId,
                    30d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        projection.Apply(
            first);

        Assert.Equal(
            70d,
            projection.Current);

        var second =
            projection.PreviewLoss(
                new ResourceLossRequest(
                    lifeId,
                    20d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        projection.Apply(
            second);

        Assert.Equal(
            50d,
            projection.Current);

        Assert.Equal(
            20d,
            second.Result.ActualLoss);

        Assert.Equal(
            100d,
            original.Current);

        Assert.Equal(
            0UL,
            original.Revision);
    }

    [Fact]
    public void LossThenRecovery_UsesUpdatedProjectedState()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var original =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var projection =
            new ResourceStateProjection(
                registry,
                original);

        var loss =
            projection.PreviewLoss(
                new ResourceLossRequest(
                    lifeId,
                    40d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        projection.Apply(
            loss);

        var recovery =
            projection.PreviewRecovery(
                new ResourceRecoveryRequest(
                    lifeId,
                    15d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Recovery)));

        projection.Apply(
            recovery);

        Assert.Equal(
            75d,
            projection.Current);

        Assert.Equal(
            100d,
            original.Current);
    }

    [Fact]
    public void SequentialCosts_UseRemainingProjectedResource()
    {
        var registry =
            CreateRegistry();

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var original =
            new ResourceState(
                manaId,
                current: 50d,
                maximum: 100d);

        var projection =
            new ResourceStateProjection(
                registry,
                original);

        var first =
            projection.PreviewCost(
                new ResourceCostRequest(
                    manaId,
                    30d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        Assert.True(
            first.IsAffordable);

        projection.Apply(
            first);

        Assert.Equal(
            20d,
            projection.Current);

        var second =
            projection.PreviewCost(
                new ResourceCostRequest(
                    manaId,
                    25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.SkillCost)));

        Assert.False(
            second.IsAffordable);

        Assert.Equal(
            5d,
            second.Shortfall);

        Assert.Throws<InvalidOperationException>(
            () =>
                projection.Apply(
                    second));

        Assert.Equal(
            20d,
            projection.Current);

        Assert.Equal(
            50d,
            original.Current);
    }

    [Fact]
    public void EarlierPreview_BecomesStaleAfterProjectionChanges()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var original =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var projection =
            new ResourceStateProjection(
                registry,
                original);

        var first =
            projection.PreviewLoss(
                new ResourceLossRequest(
                    lifeId,
                    10d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        var stale =
            projection.PreviewLoss(
                new ResourceLossRequest(
                    lifeId,
                    20d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        projection.Apply(
            first);

        Assert.Throws<InvalidOperationException>(
            () =>
                projection.Apply(
                    stale));

        Assert.Equal(
            90d,
            projection.Current);
    }

    [Fact]
    public void ValidateCanCommitToOriginal_SucceedsWhenOriginalIsUnchanged()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var original =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var projection =
            new ResourceStateProjection(
                registry,
                original);

        var preview =
            projection.PreviewLoss(
                new ResourceLossRequest(
                    lifeId,
                    25d,
                    new ResourceOperationProvenance(
                        ResourceOperationCause.Direct)));

        projection.Apply(
            preview);

        projection.ValidateCanCommitToOriginal();

        Assert.Equal(
            100d,
            original.Current);

        Assert.Equal(
            0UL,
            original.Revision);
    }

    [Fact]
    public void ValidateCanCommitToOriginal_RejectsChangedOriginal()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var original =
            new ResourceState(
                lifeId,
                current: 100d,
                maximum: 100d);

        var projection =
            new ResourceStateProjection(
                registry,
                original);

        original.SetValues(
            current: 90d,
            maximum: 100d);

        Assert.Throws<InvalidOperationException>(
            () =>
                projection.ValidateCanCommitToOriginal());
    }

    [Fact]
    public void Constructor_WithUnknownResourceId_Throws()
    {
        var registry =
            CreateRegistry();

        var original =
            new ResourceState(
                new ResourceId(999),
                current: 10d,
                maximum: 10d);

        Assert.Throws<KeyNotFoundException>(
            () =>
                new ResourceStateProjection(
                    registry,
                    original));
    }

    private static CompiledResourceRegistry
        CreateRegistry()
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