using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceDefinitionTests
{
    [Fact]
    public void Constructor_PreservesKey()
    {
        var key =
            ResourceKey.Parse(
                "resource.life");

        var definition =
            new ResourceDefinition(
                key);

        Assert.Equal(
            key,
            definition.Key);
    }

    [Fact]
    public void Constructor_WithDefaultKey_Throws()
    {
        ResourceKey key = default;

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceDefinition(
                    key));
    }
    [Fact]
    public void Constructor_PreservesRoles()
    {
        var definition =
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant);

        Assert.Equal(
            ResourceRole.DamageTarget |
            ResourceRole.DefeatRelevant,
            definition.Roles);
    }

    [Fact]
    public void Constructor_WithNoRoles_IsValid()
    {
        var definition =
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.utility"));

        Assert.Equal(
            ResourceRole.None,
            definition.Roles);
    }

    [Fact]
    public void Constructor_WithUnknownRole_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.life"),
                    (ResourceRole)(1 << 20)));
    }

    [Fact]
    public void HasRole_ReturnsTrueForPresentRole()
    {
        var definition =
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant);

        Assert.True(
            definition.HasRole(
                ResourceRole.DamageTarget));

        Assert.True(
            definition.HasRole(
                ResourceRole.DefeatRelevant));
    }

    [Fact]
    public void HasRole_ReturnsFalseForAbsentRole()
    {
        var definition =
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.mana"),
                ResourceRole.CostSource);

        Assert.False(
            definition.HasRole(
                ResourceRole.DamageTarget));
    }

    [Fact]
    public void HasRole_WithCombinedRoles_Throws()
    {
        var definition =
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                definition.HasRole(
                    ResourceRole.DamageTarget |
                    ResourceRole.DefeatRelevant));
    }
}