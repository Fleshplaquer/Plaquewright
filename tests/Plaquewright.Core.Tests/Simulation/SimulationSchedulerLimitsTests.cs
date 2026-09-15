using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationSchedulerLimitsTests
{
    [Fact]
    public void Constructor_PreservesValues()
    {
        var limits =
            new SimulationSchedulerLimits(
                maxQueueSize: 1000,
                maxSameTimestampWave: 50);

        Assert.Equal(
            1000,
            limits.MaxQueueSize);

        Assert.Equal(
            50u,
            limits.MaxSameTimestampWave);
    }

    [Fact]
    public void ZeroQueueSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new SimulationSchedulerLimits(
                    maxQueueSize: 0,
                    maxSameTimestampWave: 10));
    }

    [Fact]
    public void NegativeQueueSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new SimulationSchedulerLimits(
                    maxQueueSize: -1,
                    maxSameTimestampWave: 10));
    }

    [Fact]
    public void ZeroWaveLimit_IsValid()
    {
        var limits =
            new SimulationSchedulerLimits(
                maxQueueSize: 100,
                maxSameTimestampWave: 0);

        Assert.Equal(
            0u,
            limits.MaxSameTimestampWave);
    }
}