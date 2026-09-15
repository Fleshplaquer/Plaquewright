using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceStateSetTests
{
    [Fact]
    public void Constructor_PreservesProvidedResources()
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

        var set =
            new ResourceStateSet(
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 75d,
                        maximum: 100d),

                    new ResourceState(
                        manaId,
                        current: 20d,
                        maximum: 50d)
                ]);

        Assert.Equal(
            2,
            set.Count);

        Assert.Equal(
            75d,
            set.Get(lifeId).Current);

        Assert.Equal(
            20d,
            set.Get(manaId).Current);
    }

    [Fact]
    public void Constructor_AllowsSubsetOfRegistry()
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

        var set =
            new ResourceStateSet(
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        Assert.True(
            set.Contains(lifeId));

        Assert.False(
            set.Contains(manaId));

        Assert.Equal(
            1,
            set.Count);
    }

    [Fact]
    public void MissingResource_IsNotTreatedAsZeroResource()
    {
        var registry =
            CreateRegistry();

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var set =
            new ResourceStateSet(
                registry,
                []);

        Assert.False(
            set.Contains(manaId));

        Assert.Throws<KeyNotFoundException>(
            () =>
                set.Get(manaId));
    }

    [Fact]
    public void Constructor_RejectsDuplicateResourceIds()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        Assert.Throws<InvalidOperationException>(
            () =>
                new ResourceStateSet(
                    registry,
                    [
                        new ResourceState(
                            lifeId,
                            100d,
                            100d),

                        new ResourceState(
                            lifeId,
                            50d,
                            100d)
                    ]));
    }

    [Fact]
    public void Constructor_RejectsUnknownResourceId()
    {
        var registry =
            CreateRegistry();

        Assert.Throws<KeyNotFoundException>(
            () =>
                new ResourceStateSet(
                    registry,
                    [
                        new ResourceState(
                            new ResourceId(999),
                            1d,
                            1d)
                    ]));
    }

    [Fact]
    public void Constructor_RejectsNullStateCollection()
    {
        var registry =
            CreateRegistry();

        Assert.Throws<ArgumentNullException>(
            () =>
                new ResourceStateSet(
                    registry,
                    null!));
    }

    [Fact]
    public void Constructor_RejectsNullStateEntry()
    {
        var registry =
            CreateRegistry();

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceStateSet(
                    registry,
                    [
                        null!
                    ]));
    }

    [Fact]
    public void Constructor_CopiesResourceState()
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

        var set =
            new ResourceStateSet(
                registry,
                [source]);

        source.SetValues(
            current: 25d,
            maximum: 50d);

        var stored =
            set.Get(lifeId);

        Assert.Equal(
            75d,
            stored.Current);

        Assert.Equal(
            100d,
            stored.Maximum);

        Assert.NotSame(
            source,
            stored);
    }

    [Fact]
    public void States_AreOrderedByResourceId()
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

        var energyShieldId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.energy_shield"));

        var set =
            new ResourceStateSet(
                registry,
                [
                    new ResourceState(
                        manaId,
                        10d,
                        10d),

                    new ResourceState(
                        lifeId,
                        10d,
                        10d),

                    new ResourceState(
                        energyShieldId,
                        10d,
                        10d)
                ]);

        var ids =
            set.States
                .Select(state => state.Id)
                .ToArray();

        Assert.Equal(
            [
                energyShieldId,
                lifeId,
                manaId
            ],
            ids);
    }

    [Fact]
    public void EmptySet_IsValid()
    {
        var registry =
            CreateRegistry();

        var set =
            new ResourceStateSet(
                registry,
                []);

        Assert.Equal(
            0,
            set.Count);

        Assert.Empty(
            set.States);
    }

    [Fact]
    public void Contains_WithDefaultId_Throws()
    {
        var registry =
            CreateRegistry();

        var set =
            new ResourceStateSet(
                registry,
                []);

        ResourceId id = default;

        Assert.Throws<ArgumentException>(
            () =>
                set.Contains(id));
    }

    [Fact]
    public void Get_WithUnknownId_Throws()
    {
        var registry =
            CreateRegistry();

        var set =
            new ResourceStateSet(
                registry,
                []);

        Assert.Throws<KeyNotFoundException>(
            () =>
                set.Get(
                    new ResourceId(999)));
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
                ResourceRole.CostSource),

            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.energy_shield"),
                ResourceRole.DamageTarget |
                ResourceRole.ProtectionSource)
        ]);
    }
}