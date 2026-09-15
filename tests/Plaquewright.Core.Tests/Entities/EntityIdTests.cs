using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Tests.Entities;

public sealed class EntityIdTests
{
    [Fact]
    public void PositiveValue_IsValid()
    {
        var id =
            new EntityId(1UL);

        Assert.True(
            id.IsValid);

        Assert.Equal(
            1UL,
            id.Value);
    }

    [Fact]
    public void Zero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new EntityId(0UL));
    }

    [Fact]
    public void DefaultValue_IsInvalid()
    {
        EntityId id = default;

        Assert.False(
            id.IsValid);
    }

    [Fact]
    public void EqualValues_ProduceEqualIds()
    {
        Assert.Equal(
            new EntityId(42UL),
            new EntityId(42UL));
    }

    [Fact]
    public void DifferentValues_ProduceDifferentIds()
    {
        Assert.NotEqual(
            new EntityId(1UL),
            new EntityId(2UL));
    }

    [Fact]
    public void Comparison_UsesNumericValue()
    {
        var first =
            new EntityId(1UL);

        var second =
            new EntityId(2UL);

        Assert.True(
            first < second);

        Assert.True(
            second > first);
    }

    [Fact]
    public void ToString_WithValidId_ReturnsNumericValue()
    {
        var id =
            new EntityId(42UL);

        Assert.Equal(
            "42",
            id.ToString());
    }

    [Fact]
    public void DefaultToString_ReportsInvalid()
    {
        EntityId id = default;

        Assert.Equal(
            "<invalid>",
            id.ToString());
    }
}