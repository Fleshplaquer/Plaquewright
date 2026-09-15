using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceStateSetInvariantTests
{
    [Fact]
    public void InputOrder_DoesNotChangeStateEnumerationOrder()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                Definition("resource.life"),
                Definition("resource.mana"),
                Definition("resource.energy_shield")
            ]);

        var ids =
            new[]
            {
                registry.GetId(
                    ResourceKey.Parse(
                        "resource.life")),

                registry.GetId(
                    ResourceKey.Parse(
                        "resource.mana")),

                registry.GetId(
                    ResourceKey.Parse(
                        "resource.energy_shield"))
            };

        var forward =
            CreateSet(
                registry,
                ids);

        var reverse =
            CreateSet(
                registry,
                ids.Reverse());

        Assert.Equal(
            forward.States.Select(
                state => state.Id),
            reverse.States.Select(
                state => state.Id));
    }

    private static ResourceStateSet CreateSet(
        CompiledResourceRegistry registry,
        IEnumerable<ResourceId> ids)
    {
        return new ResourceStateSet(
            registry,
            ids.Select(
                id =>
                    new ResourceState(
                        id,
                        current: 10d,
                        maximum: 10d)));
    }

    private static ResourceDefinition Definition(
        string key)
    {
        return new ResourceDefinition(
            ResourceKey.Parse(key));
    }
}