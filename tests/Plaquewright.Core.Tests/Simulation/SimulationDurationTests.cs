using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationDurationTests
{
    [Fact]
    public void Zero_IsValid()
    {
        Assert.Equal(
            0L,
            SimulationDuration.Zero.Microseconds);
    }

    [Fact]
    public void FromMicroseconds_PreservesValue()
    {
        var duration =
            SimulationDuration.FromMicroseconds(
                123L);

        Assert.Equal(
            123L,
            duration.Microseconds);
    }

    [Fact]
    public void FromMilliseconds_ConvertsToMicroseconds()
    {
        var duration =
            SimulationDuration.FromMilliseconds(
                250L);

        Assert.Equal(
            250_000L,
            duration.Microseconds);
    }

    [Fact]
    public void FromSeconds_ConvertsToMicroseconds()
    {
        var duration =
            SimulationDuration.FromSeconds(
                2L);

        Assert.Equal(
            2_000_000L,
            duration.Microseconds);
    }

    [Fact]
    public void Constructor_WithNegativeValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SimulationDuration(-1L));
    }

    [Fact]
    public void Addition_AddsDurations()
    {
        var result =
            SimulationDuration.FromMilliseconds(250L)
            +
            SimulationDuration.FromMilliseconds(500L);

        Assert.Equal(
            750_000L,
            result.Microseconds);
    }

    [Fact]
    public void Subtraction_SubtractsDurations()
    {
        var result =
            SimulationDuration.FromSeconds(2L)
            -
            SimulationDuration.FromMilliseconds(500L);

        Assert.Equal(
            1_500_000L,
            result.Microseconds);
    }

    [Fact]
    public void Subtraction_ResultingInNegativeDuration_Throws()
    {
        var first =
            SimulationDuration.FromMilliseconds(
                100L);

        var second =
            SimulationDuration.FromMilliseconds(
                200L);

        Assert.Throws<InvalidOperationException>(
            () =>
            {
                _ = first - second;
            });
    }

    [Fact]
    public void Addition_WithOverflow_Throws()
    {
        var first =
            new SimulationDuration(
                long.MaxValue);

        var second =
            new SimulationDuration(1L);

        Assert.Throws<OverflowException>(
            () =>
            {
                _ = first + second;
            });
    }

    [Fact]
    public void Comparisons_UseMicroseconds()
    {
        var shorter =
            SimulationDuration.FromMilliseconds(
                100L);

        var longer =
            SimulationDuration.FromMilliseconds(
                200L);

        Assert.True(shorter < longer);
        Assert.True(longer > shorter);
        Assert.True(shorter <= longer);
        Assert.True(longer >= shorter);
    }
}