using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceCostInvariantTests
{
    [Fact]
    public void RepresentativeCosts_AreEitherFullyAffordableOrNotPaidAtAll()
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

        var id =
            registry.GetId(key);

        var currentValues = new[]
        {
            0d,
            1d,
            25d,
            50d,
            100d
        };

        var costValues = new[]
        {
            0d,
            1d,
            25d,
            50d,
            100d,
            200d
        };

        foreach (var current in currentValues)
        {
            foreach (var cost in costValues)
            {
                var state =
                    new ResourceState(
                        id,
                        current,
                        maximum: 100d);

                var preview =
                    ResourceCostOperations.Preview(
                        registry,
                        state,
                        new ResourceCostRequest(
                            id,
                            cost, new ResourceOperationProvenance(
        ResourceOperationCause.SkillCost)));

                Assert.Equal(
                    cost <= current,
                    preview.IsAffordable);

                if (preview.IsAffordable)
                {
                    Assert.Equal(
                        0d,
                        preview.Shortfall);

                    Assert.Equal(
                        current - cost,
                        preview.CurrentAfter);
                }
                else
                {
                    Assert.Equal(
                        cost - current,
                        preview.Shortfall);

                    Assert.Equal(
                        current,
                        preview.CurrentAfter);
                }

                // Preview never mutates.
                Assert.Equal(
                    current,
                    state.Current);

                Assert.Equal(
                    0UL,
                    state.Revision);
            }
        }
    }
}