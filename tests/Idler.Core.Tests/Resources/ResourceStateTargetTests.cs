using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceStateTargetTests
{
    [Fact]
    public void Constructor_ResolvesResourceFromEntity()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            new EntityRuntimeState(
                new EntityId(42UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 75d,
                        maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        Assert.Equal(
            entity.Id,
            target.EntityId);

        Assert.Equal(
            lifeId,
            target.ResourceId);

        Assert.Same(
            entity.Resources.Get(lifeId),
            target.State);

        Assert.Equal(
            75d,
            target.State.Current);
    }

    [Fact]
    public void SameResourceIdOnDifferentEntities_ProducesDistinctTargets()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var firstEntity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var secondEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 200d,
                        maximum: 200d)
                ]);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                lifeId);

        Assert.Equal(
            firstTarget.ResourceId,
            secondTarget.ResourceId);

        Assert.NotEqual(
            firstTarget.EntityId,
            secondTarget.EntityId);

        Assert.NotSame(
            firstTarget.State,
            secondTarget.State);
    }

    [Fact]
    public void Constructor_ForMissingResource_Throws()
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
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        Assert.Throws<KeyNotFoundException>(
            () =>
                new ResourceStateTarget(
                    entity,
                    manaId));
    }

    [Fact]
    public void Constructor_WithInvalidResourceId_Throws()
    {
        var registry =
            CreateRegistry();

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                []);

        ResourceId resourceId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceStateTarget(
                    entity,
                    resourceId));
    }

    [Fact]
    public void Constructor_WithNullEntity_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                new ResourceStateTarget(
                    null!,
                    new ResourceId(1)));
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