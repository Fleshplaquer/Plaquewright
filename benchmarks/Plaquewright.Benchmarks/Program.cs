using System.Diagnostics;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text.Json;
using Plaquewright.Core.Hosting;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Benchmarks;

internal static class Program
{
    private const int WarmupIterations =
        3;

    private const int MeasuredIterations =
        20;

    public static int Main()
    {
        ILoadProfile[] profiles =
        [
            new SchedulerBurstProfile(),
            new SameTimestampChainProfile(),
            new TimedModifierRefreshProfile(),
            new LedgerLongRunProfile()
        ];

        Console.WriteLine(
            "Plaquewright PW-S07 baseline");

        Console.WriteLine(
            $"Warmup: {WarmupIterations}, measured: {MeasuredIterations}");

        Console.WriteLine();

        var results =
            new List<ProfileBenchmarkResult>(
                profiles.Length);

        foreach (var profile in profiles)
        {
            Console.Write(
                $"{profile.Name}: warmup ");

            WarmUp(
                profile);

            Console.Write(
                "measure ");

            var samples =
                new ProfileSample[
                    MeasuredIterations];

            for (var index = 0;
                 index < samples.Length;
                 index++)
            {
                samples[index] =
                    Measure(
                        profile);

                Console.Write(
                    ".");
            }

            Console.WriteLine(
                " done");

            results.Add(
                ProfileBenchmarkResult.Create(
                    profile.Name,
                    samples));
        }

        Console.WriteLine();

        PrintSummary(
            results);

        var document =
            new BenchmarkDocument(
                DateTimeOffset.UtcNow,
                CreateEnvironment(),
                results);

        var outputPath =
            Path.Combine(
                Path.GetTempPath(),
                $"Plaquewright_PW-S07_Baseline_" +
                $"{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");

        File.WriteAllText(
            outputPath,
            JsonSerializer.Serialize(
                document,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                }));

        Console.WriteLine();
        Console.WriteLine(
            "JSON:");

        Console.WriteLine(
            outputPath);

        return 0;
    }

    private static void WarmUp(
        ILoadProfile profile)
    {
        for (var iteration = 0;
             iteration < WarmupIterations;
             iteration++)
        {
            var run =
                profile.Run();

            GC.KeepAlive(
                run.RetainedRoot);
        }

        ForceFullCollection();
    }

    private static ProfileSample Measure(
        ILoadProfile profile)
    {
        ForceFullCollection();

        var managedBefore =
            GC.GetTotalMemory(
                forceFullCollection: false);

        var gen0Before =
            GC.CollectionCount(0);

        var gen1Before =
            GC.CollectionCount(1);

        var gen2Before =
            GC.CollectionCount(2);

        var allocatedBefore =
            GC.GetAllocatedBytesForCurrentThread();

        var started =
            Stopwatch.GetTimestamp();

        var run =
            profile.Run();

        var elapsed =
            Stopwatch.GetElapsedTime(
                started);

        var allocatedAfter =
            GC.GetAllocatedBytesForCurrentThread();

        var gen0After =
            GC.CollectionCount(0);

        var gen1After =
            GC.CollectionCount(1);

        var gen2After =
            GC.CollectionCount(2);

        //
        // Keep only the explicitly declared retained root
        // alive while measuring post-GC retained memory.
        //
        var retainedRoot =
            run.RetainedRoot;

        ForceFullCollection();

        var managedAfterCollection =
            GC.GetTotalMemory(
                forceFullCollection: false);

        GC.KeepAlive(
            retainedRoot);

        var allocatedBytes =
            allocatedAfter -
            allocatedBefore;

        var throughput =
            elapsed.TotalSeconds > 0d
                ? run.LogicalOperations /
                  elapsed.TotalSeconds
                : double.PositiveInfinity;

        return new ProfileSample(
            ElapsedMilliseconds:
                elapsed.TotalMilliseconds,
            ThroughputOperationsPerSecond:
                throughput,
            AllocatedBytes:
                allocatedBytes,
            RetainedManagedDeltaBytes:
                managedAfterCollection -
                managedBefore,
            Gen0Collections:
                gen0After -
                gen0Before,
            Gen1Collections:
                gen1After -
                gen1Before,
            Gen2Collections:
                gen2After -
                gen2Before,
            LogicalOperations:
                run.LogicalOperations,
            PeakPendingEvents:
                run.PeakPendingEvents,
            LedgerEntries:
                run.LedgerEntries,
            TraceEntries:
                run.TraceEntries);
    }

    private static void ForceFullCollection()
    {
        GC.Collect(
            GC.MaxGeneration,
            GCCollectionMode.Forced,
            blocking: true,
            compacting: true);

        GC.WaitForPendingFinalizers();

        GC.Collect(
            GC.MaxGeneration,
            GCCollectionMode.Forced,
            blocking: true,
            compacting: true);
    }

    private static BenchmarkEnvironment
        CreateEnvironment()
    {
        return new BenchmarkEnvironment(
            GitCommit:
                Environment.GetEnvironmentVariable(
                    "PLAQUEWRIGHT_BENCHMARK_COMMIT")
                ?? "<not supplied>",
            MachineLabel:
                Environment.GetEnvironmentVariable(
                    "PLAQUEWRIGHT_BENCHMARK_MACHINE")
                ?? "<not supplied>",
            Framework:
                RuntimeInformation.FrameworkDescription,
            OperatingSystem:
                RuntimeInformation.OSDescription,
            OsArchitecture:
                RuntimeInformation.OSArchitecture.ToString(),
            ProcessArchitecture:
                RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessorCount:
                Environment.ProcessorCount,
            ServerGc:
                GCSettings.IsServerGC,
            GcLatencyMode:
                GCSettings.LatencyMode.ToString(),
            WarmupIterations:
                WarmupIterations,
            MeasuredIterations:
                MeasuredIterations);
    }

    private static void PrintSummary(
        IReadOnlyList<ProfileBenchmarkResult>
            results)
    {
        Console.WriteLine(
            $"{"Profile",-24}" +
            $"{"Ops",10}" +
            $"{"P50 ms",12}" +
            $"{"P95 ms",12}" +
            $"{"P99 ms",12}" +
            $"{"P50 ops/s",14}" +
            $"{"Alloc MB",12}" +
            $"{"Ret MB",12}" +
            $"{"GC 0/1/2",12}" +
            $"{"PeakQ",9}" +
            $"{"Ledger",9}");

        foreach (var result in results)
        {
            var summary =
                result.Summary;

            var gc =
                $"{summary.TotalGen0Collections}/" +
                $"{summary.TotalGen1Collections}/" +
                $"{summary.TotalGen2Collections}";

            Console.WriteLine(
                $"{result.Name,-24}" +
                $"{summary.LogicalOperations,10}" +
                $"{summary.DurationMilliseconds.P50,12:F3}" +
                $"{summary.DurationMilliseconds.P95,12:F3}" +
                $"{summary.DurationMilliseconds.P99,12:F3}" +
                $"{summary.ThroughputOperationsPerSecond.P50,14:F0}" +
                $"{summary.AllocatedBytes.P50 / 1024d / 1024d,12:F2}" +
                $"{summary.RetainedManagedDeltaBytes.P50 / 1024d / 1024d,12:F2}" +
                $"{gc,12}" +
                $"{summary.PeakPendingEvents,9}" +
                $"{summary.LedgerEntries,9}");
        }
    }
}

internal interface ILoadProfile
{
    string Name { get; }

    ProfileRunResult Run();
}

internal abstract record BenchmarkWorkItem
    : ISimulationWorkItem;

internal sealed record ProfileRunResult(
    long LogicalOperations,
    int PeakPendingEvents,
    int LedgerEntries,
    int TraceEntries,
    object RetainedRoot);

internal sealed record ProfileSample(
    double ElapsedMilliseconds,
    double ThroughputOperationsPerSecond,
    long AllocatedBytes,
    long RetainedManagedDeltaBytes,
    int Gen0Collections,
    int Gen1Collections,
    int Gen2Collections,
    long LogicalOperations,
    int PeakPendingEvents,
    int LedgerEntries,
    int TraceEntries);

internal sealed record MetricDistribution(
    double P50,
    double P95,
    double P99);

internal sealed record ProfileSummary(
    long LogicalOperations,
    MetricDistribution DurationMilliseconds,
    MetricDistribution ThroughputOperationsPerSecond,
    MetricDistribution AllocatedBytes,
    MetricDistribution RetainedManagedDeltaBytes,
    int TotalGen0Collections,
    int TotalGen1Collections,
    int TotalGen2Collections,
    int PeakPendingEvents,
    int LedgerEntries,
    int TraceEntries);

internal sealed record ProfileBenchmarkResult(
    string Name,
    ProfileSummary Summary,
    IReadOnlyList<ProfileSample> Samples)
{
    internal static ProfileBenchmarkResult Create(
        string name,
        IReadOnlyList<ProfileSample> samples)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            name);

        ArgumentNullException.ThrowIfNull(
            samples);

        if (samples.Count == 0)
        {
            throw new ArgumentException(
                "At least one measured sample is required.",
                nameof(samples));
        }

        var logicalOperations =
            samples[0].LogicalOperations;

        foreach (var sample in samples)
        {
            if (sample.LogicalOperations !=
                logicalOperations)
            {
                throw new InvalidOperationException(
                    "Profile logical operation count changed between samples.");
            }
        }

        var summary =
            new ProfileSummary(
                logicalOperations,
                Distribution(
                    samples.Select(
                        sample =>
                            sample.ElapsedMilliseconds)),
                Distribution(
                    samples.Select(
                        sample =>
                            sample.ThroughputOperationsPerSecond)),
                Distribution(
                    samples.Select(
                        sample =>
                            (double)sample.AllocatedBytes)),
                Distribution(
                    samples.Select(
                        sample =>
                            (double)sample.RetainedManagedDeltaBytes)),
                samples.Sum(
                    sample =>
                        sample.Gen0Collections),
                samples.Sum(
                    sample =>
                        sample.Gen1Collections),
                samples.Sum(
                    sample =>
                        sample.Gen2Collections),
                samples.Max(
                    sample =>
                        sample.PeakPendingEvents),
                samples.Max(
                    sample =>
                        sample.LedgerEntries),
                samples.Max(
                    sample =>
                        sample.TraceEntries));

        return new ProfileBenchmarkResult(
            name,
            summary,
            samples.ToArray());
    }

    private static MetricDistribution Distribution(
        IEnumerable<double> values)
    {
        var ordered =
            values
                .OrderBy(
                    value => value)
                .ToArray();

        return new MetricDistribution(
            Percentile(
                ordered,
                0.50d),
            Percentile(
                ordered,
                0.95d),
            Percentile(
                ordered,
                0.99d));
    }

    private static double Percentile(
        IReadOnlyList<double> ordered,
        double percentile)
    {
        var rank =
            (int)Math.Ceiling(
                percentile *
                ordered.Count);

        var index =
            Math.Clamp(
                rank - 1,
                0,
                ordered.Count - 1);

        return ordered[index];
    }
}

internal sealed record BenchmarkEnvironment(
    string GitCommit,
    string MachineLabel,
    string Framework,
    string OperatingSystem,
    string OsArchitecture,
    string ProcessArchitecture,
    int ProcessorCount,
    bool ServerGc,
    string GcLatencyMode,
    int WarmupIterations,
    int MeasuredIterations);

internal sealed record BenchmarkDocument(
    DateTimeOffset TimestampUtc,
    BenchmarkEnvironment Environment,
    IReadOnlyList<ProfileBenchmarkResult> Profiles);

internal static class SessionRunProbe
{
    internal static SimulationRunResult
        RunToCompletion<TWorkItem>(
            SimulationSession<TWorkItem> session,
            int initialPendingEvents,
            out int peakPendingEvents)
    {
        ArgumentNullException.ThrowIfNull(
            session);

        peakPendingEvents =
            initialPendingEvents;

        while (true)
        {
            var result =
                session.RunNext();

            peakPendingEvents =
                Math.Max(
                    peakPendingEvents,
                    result.PendingEvents);

            if (result.Status ==
                SimulationRunStatus.Completed)
            {
                return result;
            }

            if (result.Status ==
                SimulationRunStatus.BudgetExceeded)
            {
                throw new InvalidOperationException(
                    $"Benchmark profile exceeded simulation budget '{result.BudgetKind}'.");
            }
        }
    }
}