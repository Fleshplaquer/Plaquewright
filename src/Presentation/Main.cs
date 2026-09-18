using System;
using Godot;
using Plaquewright.Core.Composition;
using Plaquewright.Core.Hosting;
using Plaquewright.Core.Simulation;

namespace Plaquewright;

public partial class Main : Node
{
    private SimulationSession<HostWorkItem>? _session;

    private bool _hostInputHandled;

    public override void _Ready()
    {
        var compositionBuilder =
            new SimulationCompositionBuilder<HostWorkItem>();

        compositionBuilder.AddModule(
            "GodotHost",
            module =>
            {
                module.Handle<GodotReadyInput>(
                    (_, _) =>
                    {
                        _hostInputHandled =
                            true;
                    });
            });

        _session =
            new SimulationSession<HostWorkItem>(
                compositionBuilder.Build(),
                new SimulationSchedulerLimits(
                    maxQueueSize: 128,
                    maxSameTimestampWave: 32),
                new SimulationRunnerLimits(
                    maxProcessedEvents: 1024UL));

        //
        // Godot contributes an external host input.
        // All authoritative execution after this boundary
        // belongs to Plaquewright Core.
        //
        _session.ScheduleExternalInput(
            SimulationTime.Zero,
            new GodotReadyInput());

        var result =
            _session.RunToCompletion();

        if (result.Status !=
                SimulationRunStatus.Completed ||
            !_hostInputHandled ||
            _session.ProcessedEvents != 1UL)
        {
            throw new InvalidOperationException(
                "Plaquewright Godot host smoke execution failed.");
        }

        GD.Print(
            $"Plaquewright Godot host ready. " +
            $"SimulationTime={_session.CurrentTime}, " +
            $"ProcessedEvents={_session.ProcessedEvents}.");
    }

    private abstract class HostWorkItem
    {
    }

    private sealed class GodotReadyInput
        : HostWorkItem
    {
    }
}