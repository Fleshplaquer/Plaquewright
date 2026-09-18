using Plaquewright.Core.Composition;
using Plaquewright.Core.Hosting;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Benchmarks;

internal sealed class SchedulerBurstCyclesProfile
    : ILoadProfile
{
    private const int CycleCount =
        100;

    private const int EventsPerCycle =
        1_000;

    private const int TotalEventCount =
        CycleCount * EventsPerCycle;

    public string Name =>
        "SchedulerBurstCycles[100x1000]";

    public ProfileRunResult Run()
    {
        var nextExpectedIndex =
            0;

        var builder =
            new SimulationCompositionBuilder<
                BenchmarkWorkItem>();

        builder.AddModule(
            "SchedulerBurstCycles",
            module =>
            {
                module.Handle<CycleWorkItem>(
                    (workItem, _) =>
                    {
                        if (workItem.Index !=
                            nextExpectedIndex)
                        {
                            throw new InvalidOperationException(
                                "Scheduler cycle ordering changed.");
                        }

                        nextExpectedIndex++;
                    });
            });

        var session =
            new SimulationSession<BenchmarkWorkItem>(
                builder.Build(),
                new SimulationSchedulerLimits(
                    maxQueueSize:
                        EventsPerCycle,
                    maxSameTimestampWave:
                        0u),
                new SimulationRunnerLimits(
                    maxProcessedEvents:
                        TotalEventCount));

        var peakPendingEvents =
            0;

        for (var cycle = 0;
             cycle < CycleCount;
             cycle++)
        {
            var time =
                new SimulationTime(
                    cycle + 1L);

            var firstIndex =
                cycle *
                EventsPerCycle;

            for (var index = 0;
                 index < EventsPerCycle;
                 index++)
            {
                session.ScheduleExternalInput(
                    time,
                    new CycleWorkItem(
                        firstIndex +
                        index));
            }

            var result =
                SessionRunProbe.RunToCompletion(
                    session,
                    initialPendingEvents:
                        EventsPerCycle,
                    out var cyclePeak);

            peakPendingEvents =
                Math.Max(
                    peakPendingEvents,
                    cyclePeak);

            if (result.Status !=
                SimulationRunStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Scheduler cycle did not complete.");
            }
        }

        if (nextExpectedIndex !=
            TotalEventCount)
        {
            throw new InvalidOperationException(
                "Scheduler cycles did not execute every input.");
        }

        if (session.ProcessedEvents !=
            (ulong)TotalEventCount)
        {
            throw new InvalidOperationException(
                "Scheduler cycle processed-event count changed.");
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

    private sealed record CycleWorkItem(
        int Index)
        : BenchmarkWorkItem;
}

internal sealed class SchedulerBurstCyclesRetainedProfile
    : ILoadProfile
{
    private const int CycleCount =
        100;

    private const int EventsPerCycle =
        1_000;

    private const int TotalEventCount =
        CycleCount * EventsPerCycle;

    public string Name =>
        "SchedulerBurstCyclesRetained[100x1000]";

    public ProfileRunResult Run()
    {
        var nextExpectedIndex =
            0;

        var builder =
            new SimulationCompositionBuilder<
                BenchmarkWorkItem>();

        builder.AddModule(
            "SchedulerBurstCyclesRetained",
            module =>
            {
                module.Handle<CycleWorkItem>(
                    (workItem, _) =>
                    {
                        if (workItem.Index !=
                            nextExpectedIndex)
                        {
                            throw new InvalidOperationException(
                                "Scheduler retained-cycle ordering changed.");
                        }

                        nextExpectedIndex++;
                    });

                module.Handle<SentinelWorkItem>(
                    (_, _) =>
                    {
                    });
            });

        var session =
            new SimulationSession<BenchmarkWorkItem>(
                builder.Build(),
                new SimulationSchedulerLimits(
                    maxQueueSize:
                        EventsPerCycle + 1,
                    maxSameTimestampWave:
                        0u),
                new SimulationRunnerLimits(
                    maxProcessedEvents:
                        (ulong)TotalEventCount + 1UL));

        //
        // Keeps the scheduler non-empty between cycles,
        // so intermediate cycles cannot reach Completed
        // and therefore cannot release queue capacity.
        //
        session.ScheduleExternalInput(
            new SimulationTime(
                CycleCount + 1_000L),
            new SentinelWorkItem());

        for (var cycle = 0;
             cycle < CycleCount;
             cycle++)
        {
            var time =
                new SimulationTime(
                    cycle + 1L);

            var firstIndex =
                cycle *
                EventsPerCycle;

            for (var index = 0;
                 index < EventsPerCycle;
                 index++)
            {
                session.ScheduleExternalInput(
                    time,
                    new CycleWorkItem(
                        firstIndex +
                        index));
            }

            for (var index = 0;
                 index < EventsPerCycle;
                 index++)
            {
                var result =
                    session.RunNext();

                if (result.Status !=
                    SimulationRunStatus.InProgress)
                {
                    throw new InvalidOperationException(
                        "Retained scheduler cycle became quiescent unexpectedly.");
                }
            }
        }

        var finalResult =
            session.RunToCompletion();

        if (finalResult.Status !=
            SimulationRunStatus.Completed)
        {
            throw new InvalidOperationException(
                "Retained scheduler cycle did not complete.");
        }

        if (nextExpectedIndex !=
            TotalEventCount)
        {
            throw new InvalidOperationException(
                "Retained scheduler cycles did not execute every input.");
        }

        return new ProfileRunResult(
            LogicalOperations:
                TotalEventCount,
            PeakPendingEvents:
                EventsPerCycle + 1,
            LedgerEntries:
                0,
            TraceEntries:
                0,
            RetainedRoot:
                session);
    }

    private sealed record CycleWorkItem(
        int Index)
        : BenchmarkWorkItem;

    private sealed record SentinelWorkItem
        : BenchmarkWorkItem;
}