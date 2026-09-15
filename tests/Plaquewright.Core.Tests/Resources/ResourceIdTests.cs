using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceIdTests
{
    [Fact]
    public void PositiveValue_IsValid()
    {
        var id =
            new ResourceId(1);

        Assert.True(id.IsValid);

        Assert.Equal(
            1,
            id.Value);
    }

    [Fact]
    public void Zero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ResourceId(0));
    }

    [Fact]
    public void NegativeValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ResourceId(-1));
    }

    [Fact]
    public void DefaultValue_IsInvalid()
    {
        ResourceId id = default;

        Assert.False(
            id.IsValid);
    }

    [Fact]
    public void Comparison_UsesNumericValue()
    {
        var first =
            new ResourceId(1);

        var second =
            new ResourceId(2);

        Assert.True(
            first.CompareTo(second) < 0);
    }

    [Fact]
    public void ParsedKey_IsValid()
    {
        var key =
            ResourceKey.Parse(
                "resource.life");

        Assert.True(
            key.IsValid);
    }

    [Fact]
    public void DefaultKey_IsInvalid()
    {
        ResourceKey key = default;

        Assert.False(
            key.IsValid);
    }
}