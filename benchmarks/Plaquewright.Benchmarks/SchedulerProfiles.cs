using Plaquewright.Core.Composition;
using Plaquewright.Core.Hosting;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Benchmarks;

internal sealed class SchedulerBurstProfile
    : ILoadProfile
{
    private const int EventCount =
        10_000;

    public string Name =>
        "SchedulerBurst";

    public ProfileRunResult Run()
    {
        var nextExpectedIndex =
            0;

        var builder =
            new SimulationCompositionBuilder<
                BenchmarkWorkItem>();

        builder.AddModule(
            "SchedulerBurst",
            module =>
            {
                module.Handle<BurstWorkItem>(
                    (workItem, _) =>
                    {
                        if (workItem.Index !=
                            nextExpectedIndex)
                        {
                            throw new InvalidOperationException(
                                "Scheduler burst ordering changed.");
                        }

                        nextExpectedIndex++;
                    });
            });

        var session =
            new SimulationSession<BenchmarkWorkItem>(
                builder.Build(),
                new SimulationSchedulerLimits(
                    maxQueueSize:
                        EventCount,
                    maxSameTimestampWave:
                        0u),
                new SimulationRunnerLimits(
                    maxProcessedEvents:
                        EventCount));

        var time =
            new SimulationTime(
                100L);

        for (var index = 0;
             index < EventCount;
             index++)
        {
            session.ScheduleExternalInput(
                time,
                new BurstWorkItem(
                    index));
        }

        var result =
            SessionRunProbe.RunToCompletion(
                session,
                initialPendingEvents:
                    EventCount,
                out var peakPendingEvents);

        if (nextExpectedIndex !=
            EventCount)
        {
            throw new InvalidOperationException(
                "Scheduler burst did not execute every input.");
        }

        if (result.ProcessedEvents !=
            (ulong)EventCount)
        {
            throw new InvalidOperationException(
                "Scheduler burst processed-event count changed.");
        }

        return new ProfileRunResult(
            LogicalOperations:
                EventCount,
            PeakPendingEvents:
                peakPendingEvents,
            LedgerEntries:
                0,
            TraceEntries:
                0,
            RetainedRoot:
                session);
    }

    private sealed record BurstWorkItem(
        int Index)
        : BenchmarkWorkItem;
}

internal sealed class SameTimestampChainProfile
    : ILoadProfile
{
    private const int FollowUpCount =
        2_000;

    private const int TotalEventCount =
        FollowUpCount + 1;

    public string Name =>
        "SameTimestampChain";

    public ProfileRunResult Run()
    {
        var executed =
            0;

        uint expectedWave =
            0u;

        var builder =
            new SimulationCompositionBuilder<
                BenchmarkWorkItem>();

        builder.AddModule(
            "SameTimestampChain",
            module =>
            {
                module.Handle<ChainWorkItem>(
                    (workItem, context) =>
                    {
                        if (context.Key.Wave.Value !=
                            expectedWave)
                        {
                            throw new InvalidOperationException(
                                "Same-timestamp wave ordering changed.");
                        }

                        expectedWave++;
                        executed++;

                        if (workItem.RemainingFollowUps ==
                            0)
                        {
                            return;
                        }

                        context.Schedule(
                            context.CurrentTime,
                            SchedulerPhase.FollowUp,
                            new ChainWorkItem(
                                workItem.RemainingFollowUps -
                                1));
                    });
            });

        var session =
            new SimulationSession<BenchmarkWorkItem>(
                builder.Build(),
                new SimulationSchedulerLimits(
                    maxQueueSize:
                        1,
                    maxSameTimestampWave:
                        FollowUpCount),
                new SimulationRunnerLimits(
                    maxProcessedEvents:
                        TotalEventCount));

        session.ScheduleExternalInput(
            new SimulationTime(
                100L),
            new ChainWorkItem(
                FollowUpCount));

        var result =
            SessionRunProbe.RunToCompletion(
                session,
                initialPendingEvents:
                    1,
                out var peakPendingEvents);

        if (executed !=
            TotalEventCount)
        {
            throw new InvalidOperationException(
                "Same-timestamp chain event count changed.");
        }

        if (result.ProcessedEvents !=
            (ulong)TotalEventCount)
        {
            throw new InvalidOperationException(
                "Same-timestamp chain processed-event count changed.");
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

    private sealed record ChainWorkItem(
        int RemainingFollowUps)
        : BenchmarkWorkItem;
}