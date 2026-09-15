using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class CompiledResourceDefinitionTests
{
    [Fact]
    public void Compiler_PreservesIdentityAndRoles()
    {
        var key =
            ResourceKey.Parse(
                "resource.life");

        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    key,
                    ResourceRole.DamageTarget |
                    ResourceRole.DefeatRelevant)
            ]);

        var definition =
            registry.GetDefinition(key);

        Assert.Equal(
            new ResourceId(1),
            definition.Id);

        Assert.Equal(
            key,
            definition.Key);

        Assert.True(
            definition.HasRole(
                ResourceRole.DamageTarget));

        Assert.True(
            definition.HasRole(
                ResourceRole.DefeatRelevant));

        Assert.False(
            definition.HasRole(
                ResourceRole.CostSource));
    }

    [Fact]
    public void Roles_DoNotImplicitlyEnableOtherRoles()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.soul"),
                    ResourceRole.DefeatRelevant)
            ]);

        var definition =
            registry.GetDefinition(
                ResourceKey.Parse(
                    "resource.soul"));

        Assert.True(
            definition.HasRole(
                ResourceRole.DefeatRelevant));

        Assert.False(
            definition.HasRole(
                ResourceRole.DamageTarget));

        Assert.False(
            definition.HasRole(
                ResourceRole.CostSource));

        Assert.False(
            definition.HasRole(
                ResourceRole.ProtectionSource));
    }
    [Fact]
    public void HasRole_WithUnknownRole_Throws()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget)
            ]);

        var definition =
            registry.GetDefinition(
                ResourceKey.Parse(
                    "resource.life"));

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                definition.HasRole(
                    (ResourceRole)(1 << 20)));
    }
}