using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class SimulationRunnerLimitsTests
{
    [Fact]
    public void Constructor_PreservesMaximumProcessedEvents()
    {
        var limits =
            new SimulationRunnerLimits(
                1234UL);

        Assert.Equal(
            1234UL,
            limits.MaxProcessedEvents);
    }

    [Fact]
    public void ZeroProcessedEventLimit_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new SimulationRunnerLimits(
                    0UL));
    }
}