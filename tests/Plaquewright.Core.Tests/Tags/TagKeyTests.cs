using Plaquewright.Core.Tags;

namespace Plaquewright.Core.Tests.Tags;

public sealed class TagKeyTests
{
    [Theory]
    [InlineData("damage.fire")]
    [InlineData("skill.projectile")]
    [InlineData("weapon.two_handed_sword")]
    [InlineData("resource.utility.1")]
    [InlineData("fire")]
    public void Parse_WithValidKey_ReturnsTagKey(string value)
    {
        var tagKey = TagKey.Parse(value);

        Assert.Equal(value, tagKey.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".damage.fire")]
    [InlineData("damage.fire.")]
    [InlineData("damage..fire")]
    [InlineData("Damage.Fire")]
    [InlineData("damage fire")]
    [InlineData("damage-fire")]
    public void Parse_WithInvalidKey_ThrowsFormatException(string value)
    {
        Assert.Throws<FormatException>(() => TagKey.Parse(value));
    }

    [Fact]
    public void EqualKeys_AreEqual()
    {
        var first = TagKey.Parse("damage.fire");
        var second = TagKey.Parse("damage.fire");

        Assert.Equal(first, second);
    }

    [Fact]
    public void DifferentKeys_AreNotEqual()
    {
        var fire = TagKey.Parse("damage.fire");
        var cold = TagKey.Parse("damage.cold");

        Assert.NotEqual(fire, cold);
    }

    [Fact]
    public void TryParse_WithInvalidKey_ReturnsFalse()
    {
        var success = TagKey.TryParse("damage..fire", out _);

        Assert.False(success);
    }

    [Fact]
    public void ToString_ReturnsOriginalValue()
    {
        var tagKey = TagKey.Parse("damage.fire");

        Assert.Equal("damage.fire", tagKey.ToString());
    }
}