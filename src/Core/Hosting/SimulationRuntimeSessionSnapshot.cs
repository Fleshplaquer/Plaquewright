using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Hosting;

internal sealed class SimulationRuntimeSessionSnapshot<
    TPayloadSnapshot>
{
    public SimulationRuntimeStateSnapshot Runtime { get; }

    public SimulationRunnerSnapshot<TPayloadSnapshot>
        Runner
    { get; }

    private SimulationRuntimeSessionSnapshot(
        SimulationRuntimeStateSnapshot runtime,
        SimulationRunnerSnapshot<TPayloadSnapshot> runner)
    {
        ArgumentNullException.ThrowIfNull(
            runtime);

        ArgumentNullException.ThrowIfNull(
            runner);

        Runtime =
            runtime;

        Runner =
            runner;
    }

    internal static SimulationRuntimeSessionSnapshot<
        TPayloadSnapshot> Capture<TWorkItem>(
            SimulationRuntimeState runtime,
            SimulationSession<TWorkItem> session,
            Func<TWorkItem, TPayloadSnapshot> capturePayload)
    {
        ArgumentNullException.ThrowIfNull(
            runtime);

        ArgumentNullException.ThrowIfNull(
            session);

        ArgumentNullException.ThrowIfNull(
            capturePayload);

        //
        // Capture the execution side first so its existing
        // continuation/quiescence checks run before the
        // domain/runtime snapshot is returned.
        //
        var runnerSnapshot =
            session.CaptureRunnerSnapshot(
                capturePayload);

        var runtimeSnapshot =
            runtime.CaptureSnapshot();

        return new SimulationRuntimeSessionSnapshot<
            TPayloadSnapshot>(
                runtimeSnapshot,
                runnerSnapshot);
    }
}