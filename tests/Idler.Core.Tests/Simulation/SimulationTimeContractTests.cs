using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class SimulationTimeContractTests
{
    [Fact]
    public void OneSecond_EqualsOneMillionMicroseconds()
    {
        var duration =
            SimulationDuration.FromSeconds(1L);

        Assert.Equal(
            1_000_000L,
            duration.Microseconds);
    }

    [Fact]
    public void OneMillisecond_EqualsOneThousandMicroseconds()
    {
        var duration =
            SimulationDuration.FromMilliseconds(1L);

        Assert.Equal(
            1_000L,
            duration.Microseconds);
    }

    [Fact]
    public void AdvancingAndMeasuringTime_RoundTripsExactly()
    {
        var start =
            new SimulationTime(
                123_456L);

        var duration =
            new SimulationDuration(
                987_654L);

        var end =
            start + duration;

        Assert.Equal(
            duration,
            end - start);
    }
}