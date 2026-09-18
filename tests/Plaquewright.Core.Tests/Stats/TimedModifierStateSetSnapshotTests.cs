using Plaquewright.Core.Simulation;
using Plaquewright.Core.Stats;

namespace Plaquewright.Core.Tests.Stats;

public sealed class TimedModifierStateSetSnapshotTests
{
    [Fact]
    public void CaptureRestore_PreservesStateAndIndependentContinuation()
    {
        var original =
            new TimedModifierStateSet();

        var activeKey =
            TimedModifierKey.Parse(
                "status.active");

        var cancelledKey =
            TimedModifierKey.Parse(
                "status.cancelled");

        original.ApplyOrRefresh(
            activeKey,
            ModifierKind.Flat,
            25d,
            SimulationTime.Zero,
            new SimulationDuration(100L));

        original.ApplyOrRefresh(
            cancelledKey,
            ModifierKind.Flat,
            10d,
            SimulationTime.Zero,
            new SimulationDuration(100L));

        Assert.True(
            original.Cancel(
                cancelledKey));

        Assert.Equal(
            3UL,
            original.Revision);

        var cache =
            new TimedModifierValueQueryCache();

        Assert.Equal(
            125d,
            cache.Evaluate(
                100d,
                original));

        var snapshot =
            original.CaptureSnapshot();

        //
        // Mutating the source after capture must not affect
        // the captured continuation.
        //
        original.ApplyOrRefresh(
            activeKey,
            ModifierKind.Flat,
            50d,
            new SimulationTime(25L),
            new SimulationDuration(100L));

        var restored =
            TimedModifierStateSet.Restore(
                snapshot);

        Assert.NotSame(
            original,
            restored);

        Assert.Equal(
            3UL,
            restored.Revision);

        Assert.Equal(
            1,
            restored.ActiveCount);

        Assert.True(
            restored.IsActive(
                activeKey));

        Assert.False(
            restored.IsActive(
                cancelledKey));

        Assert.Equal(
            125d,
            TimedModifierValueQuery.Evaluate(
                100d,
                restored));

        Assert.Equal(
            150d,
            TimedModifierValueQuery.Evaluate(
                100d,
                original));

        //
        // Same revision + same query input is not enough:
        // restored domain state has a different identity.
        //
        Assert.False(
            cache.TryGetCached(
                100d,
                restored,
                out _));

        Assert.Equal(
            125d,
            cache.Evaluate(
                100d,
                restored));
    }

    [Fact]
    public void Restore_PreservesPendingExpirationContinuation()
    {
        var original =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "status.flat");

        var expiration =
            original.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                25d,
                SimulationTime.Zero,
                new SimulationDuration(100L));

        var snapshot =
            original.CaptureSnapshot();

        var restored =
            TimedModifierStateSet.Restore(
                snapshot);

        Assert.True(
            restored.Expire(
                expiration,
                new SimulationTime(100L)));

        Assert.False(
            restored.IsActive(
                key));

        Assert.Equal(
            2UL,
            restored.Revision);

        Assert.Equal(
            100d,
            TimedModifierValueQuery.Evaluate(
                100d,
                restored));

        //
        // Runtime A remains independent.
        //
        Assert.True(
            original.IsActive(
                key));

        Assert.Equal(
            125d,
            TimedModifierValueQuery.Evaluate(
                100d,
                original));
    }

    [Fact]
    public void Restore_PreservesRefreshGenerationAndStaleExpiration()
    {
        var original =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "status.flat");

        var first =
            original.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                25d,
                SimulationTime.Zero,
                new SimulationDuration(100L));

        var second =
            original.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                40d,
                new SimulationTime(50L),
                new SimulationDuration(100L));

        var restored =
            TimedModifierStateSet.Restore(
                original.CaptureSnapshot());

        var revisionBefore =
            restored.Revision;

        Assert.False(
            restored.Expire(
                first,
                new SimulationTime(100L)));

        Assert.Equal(
            revisionBefore,
            restored.Revision);

        Assert.True(
            restored.IsActive(
                key));

        Assert.Equal(
            140d,
            TimedModifierValueQuery.Evaluate(
                100d,
                restored));

        Assert.True(
            restored.Expire(
                second,
                new SimulationTime(150L)));

        Assert.False(
            restored.IsActive(
                key));

        Assert.Equal(
            revisionBefore + 1UL,
            restored.Revision);
    }

    [Fact]
    public void Restore_PreservesCancelledSlotGeneration()
    {
        var original =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "status.flat");

        var oldExpiration =
            original.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                25d,
                SimulationTime.Zero,
                new SimulationDuration(100L));

        Assert.True(
            original.Cancel(
                key));

        var restored =
            TimedModifierStateSet.Restore(
                original.CaptureSnapshot());

        Assert.False(
            restored.IsActive(
                key));

        var newExpiration =
            restored.ApplyOrRefresh(
                key,
                ModifierKind.Flat,
                30d,
                new SimulationTime(50L),
                new SimulationDuration(100L));

        //
        // The inactive slot itself must have survived restore.
        // Otherwise generation would incorrectly restart at 1.
        //
        Assert.Equal(
            2UL,
            newExpiration.Generation);

        Assert.False(
            restored.Expire(
                oldExpiration,
                new SimulationTime(100L)));

        Assert.True(
            restored.IsActive(
                key));

        Assert.Equal(
            130d,
            TimedModifierValueQuery.Evaluate(
                100d,
                restored));
    }

    [Fact]
    public void Restore_PreservesRevisionAndGenerationExhaustion()
    {
        var revisionExhausted =
            TimedModifierStateSet.Restore(
                new TimedModifierStateSetSnapshot(
                    ulong.MaxValue,
                    Array.Empty<
                        TimedModifierSlotSnapshot>()));

        Assert.Throws<InvalidOperationException>(
            () =>
                revisionExhausted.ApplyOrRefresh(
                    TimedModifierKey.Parse(
                        "status.revision"),
                    ModifierKind.Flat,
                    1d,
                    SimulationTime.Zero,
                    new SimulationDuration(100L)));

        Assert.Equal(
            ulong.MaxValue,
            revisionExhausted.Revision);

        Assert.Equal(
            0,
            revisionExhausted.ActiveCount);

        var generationKey =
            TimedModifierKey.Parse(
                "status.generation");

        var generationExhausted =
            TimedModifierStateSet.Restore(
                new TimedModifierStateSetSnapshot(
                    revision: 7UL,
                    [
                        new TimedModifierSlotSnapshot(
                            generationKey,
                            ulong.MaxValue,
                            active: null)
                    ]));

        Assert.Throws<InvalidOperationException>(
            () =>
                generationExhausted.ApplyOrRefresh(
                    generationKey,
                    ModifierKind.Flat,
                    1d,
                    SimulationTime.Zero,
                    new SimulationDuration(100L)));

        Assert.Equal(
            7UL,
            generationExhausted.Revision);

        Assert.False(
            generationExhausted.IsActive(
                generationKey));
    }
}