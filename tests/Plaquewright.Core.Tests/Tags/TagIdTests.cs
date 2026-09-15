using Plaquewright.Core.Tags;

namespace Plaquewright.Core.Tests.Tags;

public sealed class TagIdTests
{
    [Fact]
    public void Constructor_WithPositiveValue_IsValid()
    {
        var id = new TagId(42);

        Assert.True(id.IsValid);
        Assert.Equal(42, id.Value);
    }

    [Fact]
    public void Constructor_WithZero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TagId(0));
    }

    [Fact]
    public void Constructor_WithNegativeValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new TagId(-1));
    }

    [Fact]
    public void DefaultTagId_IsInvalid()
    {
        TagId id = default;

        Assert.False(id.IsValid);
        Assert.Equal(0, id.Value);
    }

    [Fact]
    public void EqualIds_AreEqual()
    {
        var first = new TagId(7);
        var second = new TagId(7);

        Assert.Equal(first, second);
    }

    [Fact]
    public void DifferentIds_AreNotEqual()
    {
        Assert.NotEqual(
            new TagId(7),
            new TagId(8));
    }

    [Fact]
    public void ToString_WithValidId_ReturnsNumericValue()
    {
        var id = new TagId(17);

        Assert.Equal("17", id.ToString());
    }

    [Fact]
    public void ToString_WithDefaultId_ReturnsInvalid()
    {
        TagId id = default;

        Assert.Equal("<invalid>", id.ToString());
    }
}