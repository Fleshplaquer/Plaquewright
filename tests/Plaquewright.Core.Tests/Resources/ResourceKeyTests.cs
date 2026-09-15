using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceKeyTests
{
    [Theory]
    [InlineData("resource.life")]
    [InlineData("resource.mana")]
    [InlineData("resource.energy_shield")]
    [InlineData("resource.utility.1")]
    public void Parse_WithValidValue_Succeeds(
        string value)
    {
        var key =
            ResourceKey.Parse(value);

        Assert.Equal(
            value,
            key.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".resource")]
    [InlineData("resource.")]
    [InlineData("resource..life")]
    [InlineData("Resource.Life")]
    [InlineData("resource-life")]
    public void Parse_WithInvalidValue_Throws(
        string value)
    {
        Assert.Throws<FormatException>(
            () => ResourceKey.Parse(value));
    }

    [Fact]
    public void EqualKeys_AreEqual()
    {
        Assert.Equal(
            ResourceKey.Parse("resource.life"),
            ResourceKey.Parse("resource.life"));
    }

    [Fact]
    public void DifferentKeys_AreNotEqual()
    {
        Assert.NotEqual(
            ResourceKey.Parse("resource.life"),
            ResourceKey.Parse("resource.mana"));
    }

    [Fact]
    public void DefaultValue_DoesNotEqualValidKey()
    {
        ResourceKey invalid = default;

        Assert.NotEqual(
            invalid,
            ResourceKey.Parse("resource.life"));
    }
}