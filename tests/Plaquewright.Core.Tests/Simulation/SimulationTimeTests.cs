using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationTimeTests
{
    [Fact]
    public void Zero_IsValid()
    {
        Assert.Equal(
            0L,
            SimulationTime.Zero.Microseconds);
    }

    [Fact]
    public void Constructor_WithNegativeValue_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new SimulationTime(-1L));
    }

    [Fact]
    public void AddingDuration_AdvancesTime()
    {
        var time =
            new SimulationTime(
                1_000_000L);

        var duration =
            SimulationDuration.FromMilliseconds(
                250L);

        var result =
            time + duration;

        Assert.Equal(
            1_250_000L,
            result.Microseconds);
    }

    [Fact]
    public void SubtractingDuration_MovesTimeBack()
    {
        var time =
            new SimulationTime(
                1_000_000L);

        var duration =
            SimulationDuration.FromMilliseconds(
                250L);

        var result =
            time - duration;

        Assert.Equal(
            750_000L,
            result.Microseconds);
    }

    [Fact]
    public void SubtractingTooLargeDuration_Throws()
    {
        var time =
            new SimulationTime(
                100L);

        var duration =
            new SimulationDuration(
                101L);

        Assert.Throws<InvalidOperationException>(
            () =>
            {
                _ = time - duration;
            });
    }

    [Fact]
    public void SubtractingTimes_ReturnsDuration()
    {
        var earlier =
            new SimulationTime(
                250_000L);

        var later =
            new SimulationTime(
                1_000_000L);

        var result =
            later - earlier;

        Assert.Equal(
            750_000L,
            result.Microseconds);
    }

    [Fact]
    public void SubtractingLaterTimeFromEarlierTime_Throws()
    {
        var earlier =
            new SimulationTime(
                250_000L);

        var later =
            new SimulationTime(
                1_000_000L);

        Assert.Throws<InvalidOperationException>(
            () =>
            {
                _ = earlier - later;
            });
    }

    [Fact]
    public void AddingDuration_WithOverflow_Throws()
    {
        var time =
            new SimulationTime(
                long.MaxValue);

        var duration =
            new SimulationDuration(1L);

        Assert.Throws<OverflowException>(
            () =>
            {
                _ = time + duration;
            });
    }

    [Fact]
    public void Comparisons_UseMicroseconds()
    {
        var earlier =
            new SimulationTime(
                100L);

        var later =
            new SimulationTime(
                200L);

        Assert.True(earlier < later);
        Assert.True(later > earlier);
        Assert.True(earlier <= later);
        Assert.True(later >= earlier);
    }

    [Fact]
    public void EqualTimes_AreEqual()
    {
        Assert.Equal(
            new SimulationTime(42L),
            new SimulationTime(42L));
    }
}