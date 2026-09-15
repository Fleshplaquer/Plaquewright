using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceCostRepresentabilityTests
{
    [Fact]
    public void Preview_AffordableAndRepresentableCost_IsPayable()
    {
        var setup =
            CreateSetup(
                current: 100d,
                maximum: 100d);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                CreateRequest(
                    setup.ResourceId,
                    0.1d));

        Assert.True(
            preview.IsAffordable);

        Assert.True(
            preview.IsRepresentable);

        Assert.True(
            preview.IsPayable);

        Assert.Equal(
            0d,
            preview.Shortfall);

        Assert.True(
            preview.CurrentAfter <
            preview.CurrentBefore);
    }

    [Fact]
    public void Preview_AffordableButUnrepresentableCost_IsNotPayable()
    {
        var setup =
            CreateSetup(
                current: 1e16d,
                maximum: 1e16d);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                CreateRequest(
                    setup.ResourceId,
                    1d));

        Assert.True(
            preview.IsAffordable);

        Assert.False(
            preview.IsRepresentable);

        Assert.False(
            preview.IsPayable);

        Assert.Equal(
            0d,
            preview.Shortfall);

        Assert.Equal(
            preview.CurrentBefore,
            preview.CurrentAfter);
    }

    [Fact]
    public void Preview_UnaffordableCost_IsNotPayable()
    {
        var setup =
            CreateSetup(
                current: 50d,
                maximum: 100d);

        var preview =
            ResourceCostOperations.Preview(
                setup.Registry,
                setup.State,
                CreateRequest(
                    setup.ResourceId,
                    100d));

        Assert.False(
            preview.IsAffordable);

        Assert.False(
            preview.IsRepresentable);

        Assert.False(
            preview.IsPayable);

        Assert.Equal(
            50d,
            preview.Shortfall);

        Assert.Equal(
            50d,
            preview.CurrentAfter);
    }

    private static ResourceCostRequest CreateRequest(
        ResourceId resourceId,
        double amount)
    {
        return new ResourceCostRequest(
            resourceId,
            amount,
            new ResourceOperationProvenance(
                ResourceOperationCause.SkillCost));
    }

    private static TestSetup CreateSetup(
        double current,
        double maximum)
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.CostSource)
            ]);

        var resourceId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        return new TestSetup(
            registry,
            resourceId,
            new ResourceState(
                resourceId,
                current,
                maximum));
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId ResourceId,
        ResourceState State);
}