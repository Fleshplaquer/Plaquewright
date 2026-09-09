using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class SchedulerWaveTests
{
    [Fact]
    public void InitialWave_IsZero()
    {
        Assert.Equal(
            0u,
            SchedulerWave.Initial.Value);
    }

    [Fact]
    public void Next_IncrementsWave()
    {
        var next =
            SchedulerWave.Initial.Next();

        Assert.Equal(
            1u,
            next.Value);
    }

    [Fact]
    public void Next_WithOverflow_Throws()
    {
        var wave =
            new SchedulerWave(uint.MaxValue);

        Assert.Throws<OverflowException>(
            () => wave.Next());
    }
}