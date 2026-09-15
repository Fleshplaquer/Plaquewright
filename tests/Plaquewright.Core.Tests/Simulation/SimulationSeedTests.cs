using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationSeedTests
{
    [Fact]
    public void Constructor_PreservesValue()
    {
        var seed =
            new SimulationSeed(123456789UL);

        Assert.Equal(
            123456789UL,
            seed.Value);
    }

    [Fact]
    public void Zero_IsValidSeed()
    {
        var seed =
            new SimulationSeed(0UL);

        Assert.Equal(
            0UL,
            seed.Value);
    }

    [Fact]
    public void EqualSeeds_AreEqual()
    {
        Assert.Equal(
            new SimulationSeed(42UL),
            new SimulationSeed(42UL));
    }
}