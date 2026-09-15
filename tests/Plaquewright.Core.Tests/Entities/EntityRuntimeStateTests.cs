using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Entities;

public sealed class EntityRuntimeStateTests
{
    [Fact]
    public void Constructor_PreservesEntityIdentityAndRegistry()
    {
        var registry =
            CreateRegistry();

        var id =
            new EntityId(42UL);

        var entity =
            new EntityRuntimeState(
                id,
                registry,
                []);

        Assert.Equal(
            id,
            entity.Id);

        Assert.Same(
            registry,
            entity.ResourceRegistry);
    }

    [Fact]
    public void Constructor_CreatesResourceStateSet()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

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
                        current: 75d,
                        maximum: 100d),

                    new ResourceState(
                        manaId,
                        current: 30d,
                        maximum: 50d)
                ]);

        Assert.Equal(
            2,
            entity.Resources.Count);

        Assert.Equal(
            75d,
            entity.Resources
                .Get(lifeId)
                .Current);

        Assert.Equal(
            30d,
            entity.Resources
                .Get(manaId)
                .Current);
    }

    [Fact]
    public void Constructor_AllowsEntityWithoutResources()
    {
        var registry =
            CreateRegistry();

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                []);

        Assert.Equal(
            0,
            entity.Resources.Count);
    }

    [Fact]
    public void Constructor_CopiesInitialResourceStates()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var source =
            new ResourceState(
                lifeId,
                current: 75d,
                maximum: 100d);

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [source]);

        source.SetValues(
            current: 25d,
            maximum: 50d);

        var stored =
            entity.Resources.Get(
                lifeId);

        Assert.NotSame(
            source,
            stored);

        Assert.Equal(
            75d,
            stored.Current);

        Assert.Equal(
            100d,
            stored.Maximum);
    }

    [Fact]
    public void SeparateEntities_HaveIndependentResourceState()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var first =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var second =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        first.Resources
            .Get(lifeId)
            .SetValues(
                current: 50d,
                maximum: 100d);

        Assert.Equal(
            50d,
            first.Resources
                .Get(lifeId)
                .Current);

        Assert.Equal(
            100d,
            second.Resources
                .Get(lifeId)
                .Current);
    }

    [Fact]
    public void Constructor_WithInvalidEntityId_Throws()
    {
        var registry =
            CreateRegistry();

        EntityId id = default;

        Assert.Throws<ArgumentException>(
            () =>
                new EntityRuntimeState(
                    id,
                    registry,
                    []));
    }

    [Fact]
    public void Constructor_WithNullRegistry_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                new EntityRuntimeState(
                    new EntityId(1UL),
                    null!,
                    []));
    }

    [Fact]
    public void Constructor_WithNullInitialResources_Throws()
    {
        var registry =
            CreateRegistry();

        Assert.Throws<ArgumentNullException>(
            () =>
                new EntityRuntimeState(
                    new EntityId(1UL),
                    registry,
                    null!));
    }

    [Fact]
    public void Constructor_RejectsResourceFromOutsideRegistry()
    {
        var registry =
            CreateRegistry();

        Assert.Throws<KeyNotFoundException>(
            () =>
                new EntityRuntimeState(
                    new EntityId(1UL),
                    registry,
                    [
                        new ResourceState(
                            new ResourceId(999),
                            current: 10d,
                            maximum: 10d)
                    ]));
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