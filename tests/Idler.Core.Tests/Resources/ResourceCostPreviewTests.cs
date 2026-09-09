using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceCostPreviewTests
{
    [Fact]
    public void AffordableCost_ProducesProjectedStateWithoutMutation()
    {
        var setup =
            CreateSetup(
                current: 100d,
                maximum: 100d);

        var request =
            new ResourceCostRequest(
                setup.State.Id,
                amount: 40d, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost));

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                request);

        Assert.True(
            preview.IsAffordable);

        Assert.Equal(
            100d,
            preview.AvailableAmount);

        Assert.Equal(
            0d,
            preview.Shortfall);

        Assert.Equal(
            100d,
            preview.CurrentBefore);

        Assert.Equal(
            60d,
            preview.CurrentAfter);

        Assert.Equal(
            100d,
            setup.State.Current);

        Assert.Equal(
            0UL,
            setup.State.Revision);
    }

    [Fact]
    public void ExactCost_IsAffordable()
    {
        var setup =
            CreateSetup(
                current: 50d,
                maximum: 100d);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                new ResourceCostRequest(
                    setup.State.Id,
                    amount: 50d, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost)));

        Assert.True(
            preview.IsAffordable);

        Assert.Equal(
            0d,
            preview.CurrentAfter);

        Assert.Equal(
            0d,
            preview.Shortfall);
    }

    [Fact]
    public void UnaffordableCost_DoesNotProjectPartialPayment()
    {
        var setup =
            CreateSetup(
                current: 50d,
                maximum: 100d);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                new ResourceCostRequest(
                    setup.State.Id,
                    amount: 100d, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost)));

        Assert.False(
            preview.IsAffordable);

        Assert.Equal(
            50d,
            preview.AvailableAmount);

        Assert.Equal(
            50d,
            preview.Shortfall);

        Assert.Equal(
            50d,
            preview.CurrentBefore);

        Assert.Equal(
            50d,
            preview.CurrentAfter);

        Assert.Equal(
            50d,
            setup.State.Current);
    }

    [Fact]
    public void ZeroCost_IsAffordable()
    {
        var setup =
            CreateSetup(
                current: 0d,
                maximum: 100d);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                new ResourceCostRequest(
                    setup.State.Id,
                    amount: 0d, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost)));

        Assert.True(
            preview.IsAffordable);

        Assert.Equal(
            0d,
            preview.Shortfall);

        Assert.Equal(
            0d,
            preview.CurrentAfter);
    }

    [Fact]
    public void Preview_RejectsResourceWithoutCostSourceRole()
    {
        var key =
            ResourceKey.Parse(
                "resource.life");

        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    key,
                    ResourceRole.DamageTarget)
            ]);

        var id =
            registry.GetId(key);

        var state =
            new ResourceState(
                id,
                current: 100d,
                maximum: 100d);

        var request =
            new ResourceCostRequest(
                id,
                amount: 10d, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Preview(
                    registry,
                    state,
                    request));

        Assert.Equal(
            100d,
            state.Current);
    }

    [Fact]
    public void Preview_RejectsRequestForDifferentResource()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.life"),
                    ResourceRole.CostSource),

                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.CostSource)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var lifeState =
            new ResourceState(
                lifeId,
                100d,
                100d);

        var manaRequest =
            new ResourceCostRequest(
                manaId,
                amount: 10d, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost));

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceCostOperations.Preview(
                    registry,
                    lifeState,
                    manaRequest));
    }

    private static (
        CompiledResourceRegistry Registry,
        ResourceState State)
        CreateSetup(
            double current,
            double maximum)
    {
        var key =
            ResourceKey.Parse(
                "resource.mana");

        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    key,
                    ResourceRole.CostSource)
            ]);

        var state =
            new ResourceState(
                registry.GetId(key),
                current,
                maximum);

        return (
            registry,
            state);
    }
}