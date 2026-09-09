using Idler.Core.Combat;

namespace Idler.Core.Tests.Combat;

public sealed class DamageExecutionIdTests
{
    [Fact]
    public void Constructor_WithPositiveValue_IsValid()
    {
        var id =
            new DamageExecutionId(
                42UL);

        Assert.True(
            id.IsValid);

        Assert.Equal(
            42UL,
            id.Value);
    }

    [Fact]
    public void Constructor_WithZero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new DamageExecutionId(
                    0UL));
    }

    [Fact]
    public void DefaultValue_IsInvalid()
    {
        DamageExecutionId id =
            default;

        Assert.False(
            id.IsValid);

        Assert.Equal(
            0UL,
            id.Value);

        Assert.Equal(
            "<invalid>",
            id.ToString());
    }

    [Fact]
    public void Equality_UsesValue()
    {
        var first =
            new DamageExecutionId(
                42UL);

        var second =
            new DamageExecutionId(
                42UL);

        var other =
            new DamageExecutionId(
                43UL);

        Assert.Equal(
            first,
            second);

        Assert.NotEqual(
            first,
            other);
    }

    [Fact]
    public void Comparison_UsesNumericValue()
    {
        var first =
            new DamageExecutionId(
                10UL);

        var second =
            new DamageExecutionId(
                20UL);

        Assert.True(
            first < second);

        Assert.True(
            first <= second);

        Assert.True(
            second > first);

        Assert.True(
            second >= first);

        Assert.Equal(
            -1,
            Math.Sign(
                first.CompareTo(
                    second)));
    }

    [Fact]
    public void ToString_ForValidId_ReturnsNumericValue()
    {
        var id =
            new DamageExecutionId(
                123UL);

        Assert.Equal(
            "123",
            id.ToString());
    }
}