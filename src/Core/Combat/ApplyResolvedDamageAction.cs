using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

public sealed class ApplyResolvedDamageAction
    : ISimulationWorkItem
{
    public DamageResolutionContext Resolution { get; }

    public ApplyResolvedDamageAction(
        DamageResolutionContext resolution)
    {
        ArgumentNullException.ThrowIfNull(
            resolution);

        Resolution =
            resolution;
    }
}