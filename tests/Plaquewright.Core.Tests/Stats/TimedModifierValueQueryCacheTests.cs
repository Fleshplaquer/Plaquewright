using Plaquewright.Core.Simulation;
using Plaquewright.Core.Stats;

namespace Plaquewright.Core.Tests.Stats;

public sealed class TimedModifierValueQueryCacheTests
{
    [Fact]
    public void Evaluate_UnchangedStateAndInputProducesCacheHit()
    {
        var state =
            new TimedModifierStateSet();

        state.ApplyOrRefresh(
            TimedModifierKey.Parse(
                "status.flat"),
            ModifierKind.Flat,
            25d,
            SimulationTime.Zero,
            new SimulationDuration(100L));

        var cache =
            new TimedModifierValueQueryCache();

        Assert.False(
            cache.TryGetCached(
                100d,
                state,
                out _));

        var first =
            cache.Evaluate(
                100d,
                state);

        Assert.Equal(
            TimedModifierValueQuery.Evaluate(
                100d,
                state),
            first);

        Assert.True(
            cache.TryGetCached(
                100d,
                state,
                out var cached));

        Assert.Equal(
            first,
            cached);

        Assert.Equal(
            first,
            cache.Evaluate(
                100d,
                state));
    }

    [Fact]
    public void AuthoritativeStateChangesInvalidateCachedValue()
    {
        var state =
            new TimedModifierStateSet();

        var cache =
            new TimedModifierValueQueryCache();

        var key =
            TimedModifierKey.Parse(
                "status.flat");

        Assert.Equal(
            100d,
            cache.Evaluate(
                100d,
                state));

        var firstExpiration =
            state.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                25d,
                SimulationTime.Zero,
                new SimulationDuration(100L));

        AssertCacheMissAndReferenceMatch(
            cache,
            state,
            100d,
            125d);

        var secondExpiration =
            state.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                40d,
                new SimulationTime(25L),
                new SimulationDuration(100L));

        AssertCacheMissAndReferenceMatch(
            cache,
            state,
            100d,
            140d);

        Assert.True(
            state.Cancel(
                key));

        AssertCacheMissAndReferenceMatch(
            cache,
            state,
            100d,
            100d);

        var thirdExpiration =
            state.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                10d,
                new SimulationTime(50L),
                new SimulationDuration(100L));

        AssertCacheMissAndReferenceMatch(
            cache,
            state,
            100d,
            110d);

        Assert.True(
            state.Expire(
                thirdExpiration,
                new SimulationTime(150L)));

        AssertCacheMissAndReferenceMatch(
            cache,
            state,
            100d,
            100d);

        //
        // Keep the earlier lifetimes visibly distinct:
        // none of them may affect the current cache contract.
        //
        Assert.NotEqual(
            firstExpiration.Generation,
            secondExpiration.Generation);
    }

    [Fact]
    public void StaleExpiration_DoesNotInvalidateCachedValue()
    {
        var state =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "status.flat");

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
                40d,
                new SimulationTime(50L),
                new SimulationDuration(100L));

        var cache =
            new TimedModifierValueQueryCache();

        var value =
            cache.Evaluate(
                100d,
                state);

        var revisionBefore =
            state.Revision;

        Assert.False(
            state.Expire(
                first,
                new SimulationTime(100L)));

        Assert.Equal(
            revisionBefore,
            state.Revision);

        Assert.True(
            cache.TryGetCached(
                100d,
                state,
                out var cached));

        Assert.Equal(
            value,
            cached);

        Assert.Equal(
            TimedModifierValueQuery.Evaluate(
                100d,
                state),
            cached);
    }

    [Fact]
    public void CacheKeyIncludesBaseValueAndStateIdentity()
    {
        var firstState =
            new TimedModifierStateSet();

        var secondState =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "status.flat");

        firstState.ApplyOrRefresh(
            key,
            ModifierKind.Flat,
            10d,
            SimulationTime.Zero,
            new SimulationDuration(100L));

        secondState.ApplyOrRefresh(
            key,
            ModifierKind.Flat,
            20d,
            SimulationTime.Zero,
            new SimulationDuration(100L));

        Assert.Equal(
            firstState.Revision,
            secondState.Revision);

        var cache =
            new TimedModifierValueQueryCache();

        Assert.Equal(
            110d,
            cache.Evaluate(
                100d,
                firstState));

        Assert.False(
            cache.TryGetCached(
                200d,
                firstState,
                out _));

        Assert.Equal(
            210d,
            cache.Evaluate(
                200d,
                firstState));

        Assert.False(
            cache.TryGetCached(
                200d,
                secondState,
                out _));

        Assert.Equal(
            220d,
            cache.Evaluate(
                200d,
                secondState));

        Assert.Equal(
            TimedModifierValueQuery.Evaluate(
                200d,
                secondState),
            cache.Evaluate(
                200d,
                secondState));
    }

    private static void AssertCacheMissAndReferenceMatch(
        TimedModifierValueQueryCache cache,
        TimedModifierStateSet state,
        double baseValue,
        double expected)
    {
        Assert.False(
            cache.TryGetCached(
                baseValue,
                state,
                out _));

        var actual =
            cache.Evaluate(
                baseValue,
                state);

        Assert.Equal(
            expected,
            actual);

        Assert.Equal(
            TimedModifierValueQuery.Evaluate(
                baseValue,
                state),
            actual);

        Assert.True(
            cache.TryGetCached(
                baseValue,
                state,
                out var cached));

        Assert.Equal(
            actual,
            cached);
    }
}