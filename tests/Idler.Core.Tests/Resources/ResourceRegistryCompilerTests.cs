using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceRegistryCompilerTests
{
    [Fact]
    public void Compile_AssignsIdsByOrdinalKeyOrder()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                Definition("resource.mana"),
                Definition("resource.life"),
                Definition("resource.energy_shield")
            ]);

        Assert.Equal(
            1,
            registry
                .GetId(
                    ResourceKey.Parse(
                        "resource.energy_shield"))
                .Value);

        Assert.Equal(
            2,
            registry
                .GetId(
                    ResourceKey.Parse(
                        "resource.life"))
                .Value);

        Assert.Equal(
            3,
            registry
                .GetId(
                    ResourceKey.Parse(
                        "resource.mana"))
                .Value);
    }

    [Fact]
    public void Compile_IsIndependentOfAuthoringOrder()
    {
        var first =
            ResourceRegistryCompiler.Compile(
            [
                Definition("resource.life"),
                Definition("resource.mana"),
                Definition("resource.energy_shield")
            ]);

        var second =
            ResourceRegistryCompiler.Compile(
            [
                Definition("resource.energy_shield"),
                Definition("resource.life"),
                Definition("resource.mana")
            ]);

        var keys = new[]
        {
            ResourceKey.Parse(
                "resource.life"),

            ResourceKey.Parse(
                "resource.mana"),

            ResourceKey.Parse(
                "resource.energy_shield")
        };

        foreach (var key in keys)
        {
            Assert.Equal(
                first.GetId(key),
                second.GetId(key));
        }
    }

    [Fact]
    public void Compile_WithDuplicateKey_Throws()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceRegistryCompiler.Compile(
                [
                    Definition("resource.life"),
                    Definition("resource.life")
                ]));
    }

    [Fact]
    public void Compile_WithNullCollection_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () =>
                ResourceRegistryCompiler.Compile(
                    null!));
    }

    [Fact]
    public void Compile_WithNullEntry_Throws()
    {
        Assert.Throws<ArgumentException>(
            () =>
                ResourceRegistryCompiler.Compile(
                [
                    Definition("resource.life"),
                    null!
                ]));
    }

    [Fact]
    public void Compile_EmptyCollection_ProducesEmptyRegistry()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
                []);

        Assert.Equal(
            0,
            registry.Count);
    }

    private static ResourceDefinition Definition(
        string key)
    {
        return new ResourceDefinition(
            ResourceKey.Parse(key));
    }
    [Fact]
    public void Compile_PreservesRolesForCorrectResourceAfterSorting()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.mana"),
                ResourceRole.CostSource),

            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant)
            ]);

        var life =
            registry.GetDefinition(
                ResourceKey.Parse(
                    "resource.life"));

        var mana =
            registry.GetDefinition(
                ResourceKey.Parse(
                    "resource.mana"));

        Assert.True(
            life.HasRole(
                ResourceRole.DamageTarget));

        Assert.True(
            life.HasRole(
                ResourceRole.DefeatRelevant));

        Assert.False(
            life.HasRole(
                ResourceRole.CostSource));

        Assert.True(
            mana.HasRole(
                ResourceRole.CostSource));

        Assert.False(
            mana.HasRole(
                ResourceRole.DefeatRelevant));
    }
}