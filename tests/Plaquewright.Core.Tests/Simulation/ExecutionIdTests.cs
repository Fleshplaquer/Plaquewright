using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class ExecutionIdTests
{
    [Fact]
    public void PositiveValue_IsValid()
    {
        var id =
            new ExecutionId(1UL);

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
                new ExecutionId(0UL));
    }

    [Fact]
    public void DefaultValue_IsInvalid()
    {
        ExecutionId id = default;

        Assert.False(
            id.IsValid);
    }

    [Fact]
    public void EqualValues_ProduceEqualIds()
    {
        Assert.Equal(
            new ExecutionId(42UL),
            new ExecutionId(42UL));
    }

    [Fact]
    public void DifferentValues_ProduceDifferentIds()
    {
        Assert.NotEqual(
            new ExecutionId(1UL),
            new ExecutionId(2UL));
    }

    [Fact]
    public void Comparison_UsesNumericValue()
    {
        var first =
            new ExecutionId(1UL);

        var second =
            new ExecutionId(2UL);

        Assert.True(
            first < second);

        Assert.True(
            second > first);
    }

    [Fact]
    public void ToString_WithValidId_ReturnsNumericValue()
    {
        var id =
            new ExecutionId(42UL);

        Assert.Equal(
            "42",
            id.ToString());
    }

    [Fact]
    public void DefaultToString_ReportsInvalid()
    {
        ExecutionId id = default;

        Assert.Equal(
            "<invalid>",
            id.ToString());
    }
}