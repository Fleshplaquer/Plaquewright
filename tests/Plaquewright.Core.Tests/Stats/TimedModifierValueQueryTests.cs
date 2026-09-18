using Plaquewright.Core.Simulation;
using Plaquewright.Core.Stats;

namespace Plaquewright.Core.Tests.Stats;

public sealed class TimedModifierValueQueryTests
{
    [Fact]
    public void Evaluate_ComposesActiveModifierFamilies()
    {
        var state =
            new TimedModifierStateSet();

        state.ApplyOrRefresh(
            TimedModifierKey.Parse(
                "status.flat"),
            ModifierKind.Flat,
            20d,
            SimulationTime.Zero,
            new SimulationDuration(100L));

        state.ApplyOrRefresh(
            TimedModifierKey.Parse(
                "status.increased"),
            ModifierKind.Increased,
            0.50d,
            SimulationTime.Zero,
            new SimulationDuration(100L));

        var result =
            TimedModifierValueQuery.Evaluate(
                100d,
                state);

        Assert.Equal(
            180d,
            result);
    }

    [Fact]
    public void Evaluate_AfterExpirationReturnsBaseValue()
    {
        var state =
            new TimedModifierStateSet();

        var expiration =
            state.ApplyOrRefresh(
                TimedModifierKey.Parse(
                    "status.flat"),
                ModifierKind.Flat,
                25d,
                SimulationTime.Zero,
                new SimulationDuration(100L));

        Assert.Equal(
            125d,
            TimedModifierValueQuery.Evaluate(
                100d,
                state));

        state.Expire(
            expiration,
            new SimulationTime(100L));

        Assert.Equal(
            100d,
            TimedModifierValueQuery.Evaluate(
                100d,
                state));
    }
}