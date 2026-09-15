using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SchedulerOrderingInvariantTests
{
    [Fact]
    public void SortingSameKeysFromDifferentInputOrders_ProducesSameOrder()
    {
        var keys = new[]
        {
            Create(200, 0, SchedulerPhase.Execution, 5),
            Create(100, 1, SchedulerPhase.StateBoundary, 2),
            Create(100, 0, SchedulerPhase.FollowUp, 4),
            Create(100, 0, SchedulerPhase.Execution, 3),
            Create(100, 0, SchedulerPhase.StateBoundary, 9),
            Create(100, 0, SchedulerPhase.Execution, 1)
        };

        var forward =
            keys
                .Order()
                .ToArray();

        var reverse =
            keys
                .Reverse()
                .Order()
                .ToArray();

        Assert.Equal(
            forward,
            reverse);
    }

    private static ScheduledEventKey Create(
        long time,
        uint wave,
        SchedulerPhase phase,
        ulong sequence)
    {
        return new ScheduledEventKey(
            new SimulationTime(time),
            new SchedulerWave(wave),
            phase,
            new ScheduledSequence(sequence));
    }
}