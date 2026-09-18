using Plaquewright.Core.Simulation;
using Plaquewright.Core.Stats;

namespace Plaquewright.Core.Tests.Stats;

public sealed class TimedModifierStateSetTests
{
    [Fact]
    public void ApplyAndExpire_ChangesStateAndRevision()
    {
        var state =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "status.resistance_boost");

        var expiration =
            state.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                25d,
                new SimulationTime(100L),
                new SimulationDuration(50L));

        Assert.True(
            state.IsActive(
                key));

        Assert.Equal(
            1,
            state.ActiveCount);

        Assert.Equal(
            1UL,
            state.Revision);

        Assert.Equal(
            new SimulationTime(150L),
            expiration.ExpiresAt);

        Assert.True(
            state.Expire(
                expiration,
                new SimulationTime(150L)));

        Assert.False(
            state.IsActive(
                key));

        Assert.Equal(
            0,
            state.ActiveCount);

        Assert.Equal(
            2UL,
            state.Revision);
    }

    [Fact]
    public void Refresh_MakesEarlierExpirationStale()
    {
        var state =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "status.resistance_boost");

        var first =
            state.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                25d,
                SimulationTime.Zero,
                new SimulationDuration(100L));

        var second =
            state.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                40d,
                new SimulationTime(50L),
                new SimulationDuration(100L));

        Assert.Equal(
            1UL,
            first.Generation);

        Assert.Equal(
            2UL,
            second.Generation);

        Assert.False(
            state.Expire(
                first,
                new SimulationTime(100L)));

        Assert.True(
            state.IsActive(
                key));

        Assert.Equal(
            40d,
            TimedModifierValueQuery.Evaluate(
                0d,
                state));

        Assert.True(
            state.Expire(
                second,
                new SimulationTime(150L)));

        Assert.False(
            state.IsActive(
                key));
    }

    [Fact]
    public void CancelThenReapply_MakesOldExpirationStale()
    {
        var state =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "status.resistance_boost");

        var first =
            state.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                25d,
                SimulationTime.Zero,
                new SimulationDuration(100L));

        Assert.True(
            state.Cancel(
                key));

        var second =
            state.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                30d,
                new SimulationTime(25L),
                new SimulationDuration(100L));

        Assert.False(
            state.Expire(
                first,
                new SimulationTime(100L)));

        Assert.True(
            state.IsActive(
                key));

        Assert.Equal(
            2UL,
            second.Generation);
    }

    [Fact]
    public void ZeroDuration_IsRejectedWithoutMutation()
    {
        var state =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "status.resistance_boost");

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                state.ApplyOrRefresh(
                    key,
                    ModifierKind.Flat,
                    25d,
                    SimulationTime.Zero,
                    SimulationDuration.Zero));

        Assert.Equal(
            0UL,
            state.Revision);

        Assert.Equal(
            0,
            state.ActiveCount);
    }

    [Fact]
    public void StaleExpiration_DoesNotAdvanceRevision()
    {
        var state =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "status.resistance_boost");

        var first =
            state.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                25d,
                SimulationTime.Zero,
                new SimulationDuration(100L));

        _ =
            state.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                30d,
                new SimulationTime(50L),
                new SimulationDuration(100L));

        var before =
            state.Revision;

        Assert.False(
            state.Expire(
                first,
                new SimulationTime(100L)));

        Assert.Equal(
            before,
            state.Revision);
    }
}