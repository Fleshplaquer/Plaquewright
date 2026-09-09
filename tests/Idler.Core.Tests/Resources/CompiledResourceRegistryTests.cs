using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class CompiledResourceRegistryTests
{
    [Fact]
    public void GetIdAndGetKey_RoundTrip()
    {
        var registry =
            CreateRegistry();

        var key =
            ResourceKey.Parse(
                "resource.life");

        var id =
            registry.GetId(key);

        Assert.Equal(
            key,
            registry.GetKey(id));
    }

    [Fact]
    public void GetId_WithUnknownKey_Throws()
    {
        var registry =
            CreateRegistry();

        Assert.Throws<KeyNotFoundException>(
            () =>
                registry.GetId(
                    ResourceKey.Parse(
                        "resource.rage")));
    }

    [Fact]
    public void GetId_WithDefaultKey_Throws()
    {
        var registry =
            CreateRegistry();

        ResourceKey key = default;

        Assert.Throws<ArgumentException>(
            () =>
                registry.GetId(key));
    }

    [Fact]
    public void GetKey_WithDefaultId_Throws()
    {
        var registry =
            CreateRegistry();

        ResourceId id = default;

        Assert.Throws<ArgumentException>(
            () =>
                registry.GetKey(id));
    }

    [Fact]
    public void GetKey_WithUnknownId_Throws()
    {
        var registry =
            CreateRegistry();

        Assert.Throws<KeyNotFoundException>(
            () =>
                registry.GetKey(
                    new ResourceId(999)));
    }

    private static CompiledResourceRegistry
        CreateRegistry()
    {
        return ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life")),

            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.mana"))
        ]);
    }
    [Fact]
    public void GetDefinition_ByKeyAndId_ReturnsSameDefinition()
    {
        var registry =
            CreateRegistry();

        var key =
            ResourceKey.Parse(
                "resource.life");

        var id =
            registry.GetId(key);

        var byKey =
            registry.GetDefinition(key);

        var byId =
            registry.GetDefinition(id);

        Assert.Same(
            byKey,
            byId);
    }
}