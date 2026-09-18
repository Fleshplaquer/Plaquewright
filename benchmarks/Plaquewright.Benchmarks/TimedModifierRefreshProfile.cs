using Plaquewright.Core.Composition;
using Plaquewright.Core.Hosting;
using Plaquewright.Core.Simulation;
using Plaquewright.Core.Stats;

namespace Plaquewright.Benchmarks;

internal sealed class TimedModifierRefreshProfile
    : ILoadProfile
{
    private const int RefreshCount =
        10_000;

    private const int TotalEventCount =
        RefreshCount * 2;

    public string Name =>
        "TimedModifierRefresh";

    public ProfileRunResult Run()
    {
        var state =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "benchmark.refresh");

        var staleExpirations =
            0;

        var effectiveExpirations =
            0;

        var builder =
            new SimulationCompositionBuilder<
                BenchmarkWorkItem>();

        builder.AddModule(
            "TimedModifierRefresh",
            module =>
            {
                module.Handle<RefreshModifierInput>(
                    (_, context) =>
                    {
                        var expiration =
                            state.ApplyOrRefresh(
                                key,
                                ModifierKind.Flat,
                                25d,
                                context.CurrentTime,
                                new SimulationDuration(
                                    1_000_000L));

                        context.Schedule(
                            expiration.ExpiresAt,
                            SchedulerPhase.StateBoundary,
                            new ExpireModifierAction(
                                expiration));
                    });

                module.Handle<ExpireModifierAction>(
                    (action, context) =>
                    {
                        var expired =
                            state.Expire(
                                action.Expiration,
                                context.CurrentTime);

                        if (expired)
                        {
                            effectiveExpirations++;
                        }
                        else
                        {
                            staleExpirations++;
                        }
                    });
            });

        var session =
            new SimulationSession<BenchmarkWorkItem>(
                builder.Build(),
                new SimulationSchedulerLimits(
                    maxQueueSize:
                        RefreshCount,
                    maxSameTimestampWave:
                        0u),
                new SimulationRunnerLimits(
                    maxProcessedEvents:
                        TotalEventCount));

        var refreshTime =
            new SimulationTime(
                100L);

        for (var index = 0;
             index < RefreshCount;
             index++)
        {
            session.ScheduleExternalInput(
                refreshTime,
                new RefreshModifierInput());
        }

        var result =
            SessionRunProbe.RunToCompletion(
                session,
                initialPendingEvents:
                    RefreshCount,
                out var peakPendingEvents);

        if (staleExpirations !=
            RefreshCount - 1)
        {
            throw new InvalidOperationException(
                "Timed modifier stale-expiration count changed.");
        }

        if (effectiveExpirations !=
            1)
        {
            throw new InvalidOperationException(
                "Timed modifier effective-expiration count changed.");
        }

        if (state.ActiveCount !=
            0)
        {
            throw new InvalidOperationException(
                "Timed modifier remained active after benchmark completion.");
        }

        if (state.Revision !=
            (ulong)RefreshCount + 1UL)
        {
            throw new InvalidOperationException(
                "Timed modifier revision count changed.");
        }

        if (TimedModifierValueQuery.Evaluate(
                100d,
                state) !=
            100d)
        {
            throw new InvalidOperationException(
                "Timed modifier final derived value changed.");
        }

        if (result.ProcessedEvents !=
            (ulong)TotalEventCount)
        {
            throw new InvalidOperationException(
                "Timed modifier processed-event count changed.");
        }

        return new ProfileRunResult(
            LogicalOperations:
                TotalEventCount,
            PeakPendingEvents:
                peakPendingEvents,
            LedgerEntries:
                0,
            TraceEntries:
                0,
            RetainedRoot:
                session);
    }

    private sealed record RefreshModifierInput
        : BenchmarkWorkItem;

    private sealed record ExpireModifierAction(
        TimedModifierExpiration Expiration)
        : BenchmarkWorkItem;
}