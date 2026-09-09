using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Entities;

public sealed class EntityRuntimeStateSetTests
{
    [Fact]
    public void NewSet_IsEmpty()
    {
        var registry =
            CreateRegistry();

        var set =
            new EntityRuntimeStateSet(
                registry);

        Assert.Equal(
            0,
            set.Count);

        Assert.Empty(
            set.Entities);

        Assert.Same(
            registry,
            set.ResourceRegistry);
    }

    [Fact]
    public void Add_StoresEntity()
    {
        var registry =
            CreateRegistry();

        var entity =
            CreateEntity(
                registry,
                1UL);

        var set =
            new EntityRuntimeStateSet(
                registry);

        set.Add(
            entity);

        Assert.Equal(
            1,
            set.Count);

        Assert.Same(
            entity,
            set.Get(entity.Id));
    }

    [Fact]
    public void Contains_ReturnsTrueForExistingEntity()
    {
        var registry =
            CreateRegistry();

        var entity =
            CreateEntity(
                registry,
                1UL);

        var set =
            new EntityRuntimeStateSet(
                registry);

        set.Add(
            entity);

        Assert.True(
            set.Contains(entity.Id));
    }

    [Fact]
    public void Contains_ReturnsFalseForUnknownEntity()
    {
        var registry =
            CreateRegistry();

        var set =
            new EntityRuntimeStateSet(
                registry);

        Assert.False(
            set.Contains(
                new EntityId(42UL)));
    }

    [Fact]
    public void Get_UnknownEntity_Throws()
    {
        var registry =
            CreateRegistry();

        var set =
            new EntityRuntimeStateSet(
                registry);

        Assert.Throws<KeyNotFoundException>(
            () =>
                set.Get(
                    new EntityId(42UL)));
    }

    [Fact]
    public void Add_DuplicateEntityId_Throws()
    {
        var registry =
            CreateRegistry();

        var first =
            CreateEntity(
                registry,
                1UL);

        var second =
            CreateEntity(
                registry,
                1UL);

        var set =
            new EntityRuntimeStateSet(
                registry);

        set.Add(
            first);

        Assert.Throws<ArgumentException>(
            () =>
                set.Add(
                    second));

        Assert.Equal(
            1,
            set.Count);

        Assert.Same(
            first,
            set.Get(first.Id));
    }

    [Fact]
    public void Add_EntityFromDifferentRegistry_Throws()
    {
        var firstRegistry =
            CreateRegistry();

        var secondRegistry =
            CreateRegistry();

        var entity =
            CreateEntity(
                secondRegistry,
                1UL);

        var set =
            new EntityRuntimeStateSet(
                firstRegistry);

        Assert.Throws<ArgumentException>(
            () =>
                set.Add(
                    entity));

        Assert.Equal(
            0,
            set.Count);
    }

    [Fact]
    public void Entities_AreEnumeratedInEntityIdOrder()
    {
        var registry =
            CreateRegistry();

        var set =
            new EntityRuntimeStateSet(
                registry);

        set.Add(
            CreateEntity(
                registry,
                5UL));

        set.Add(
            CreateEntity(
                registry,
                2UL));

        set.Add(
            CreateEntity(
                registry,
                9UL));

        var ids =
            set.Entities
                .Select(entity => entity.Id)
                .ToArray();

        Assert.Equal(
            [
                new EntityId(2UL),
                new EntityId(5UL),
                new EntityId(9UL)
            ],
            ids);
    }

    [Fact]
    public void InvalidEntityId_IsRejectedByContains()
    {
        var registry =
            CreateRegistry();

        var set =
            new EntityRuntimeStateSet(
                registry);

        EntityId id = default;

        Assert.Throws<ArgumentException>(
            () =>
                set.Contains(id));
    }

    [Fact]
    public void InvalidEntityId_IsRejectedByGet()
    {
        var registry =
            CreateRegistry();

        var set =
            new EntityRuntimeStateSet(
                registry);

        EntityId id = default;

        Assert.Throws<ArgumentException>(
            () =>
                set.Get(id));
    }

    [Fact]
    public void Constructor_WithNullRegistry_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                new EntityRuntimeStateSet(
                    null!));
    }

    [Fact]
    public void Add_WithNullEntity_Throws()
    {
        var registry =
            CreateRegistry();

        var set =
            new EntityRuntimeStateSet(
                registry);

        Assert.Throws<ArgumentNullException>(
            () =>
                set.Add(
                    null!));
    }

    private static EntityRuntimeState CreateEntity(
        CompiledResourceRegistry registry,
        ulong id)
    {
        return new EntityRuntimeState(
            new EntityId(id),
            registry,
            []);
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