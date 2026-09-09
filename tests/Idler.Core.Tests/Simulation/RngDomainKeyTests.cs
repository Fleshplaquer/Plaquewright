using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class RngDomainKeyTests
{
    [Theory]
    [InlineData("roll.damage")]
    [InlineData("roll.crit")]
    [InlineData("roll.trigger")]
    [InlineData("skill.projectile.1")]
    public void Parse_WithValidValue_Succeeds(
        string value)
    {
        var key =
            RngDomainKey.Parse(value);

        Assert.Equal(
            value,
            key.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(".roll")]
    [InlineData("roll.")]
    [InlineData("roll..crit")]
    [InlineData("Roll.Crit")]
    [InlineData("roll-crit")]
    public void Parse_WithInvalidValue_Throws(
        string value)
    {
        Assert.Throws<FormatException>(
            () => RngDomainKey.Parse(value));
    }

    [Fact]
    public void EqualKeys_AreEqual()
    {
        Assert.Equal(
            RngDomainKey.Parse("roll.crit"),
            RngDomainKey.Parse("roll.crit"));
    }
}