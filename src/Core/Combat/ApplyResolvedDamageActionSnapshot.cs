using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

internal sealed class ApplyResolvedDamageActionSnapshot
{
    public DamageResolutionSnapshot Resolution { get; }

    private ApplyResolvedDamageActionSnapshot(
        DamageResolutionSnapshot resolution)
    {
        ArgumentNullException.ThrowIfNull(
            resolution);

        Resolution =
            resolution;
    }

    public static ApplyResolvedDamageActionSnapshot Capture(
        ApplyResolvedDamageAction action)
    {
        ArgumentNullException.ThrowIfNull(
            action);

        return new ApplyResolvedDamageActionSnapshot(
            DamageResolutionSnapshot.Capture(
                action.Resolution));
    }

    public ApplyResolvedDamageAction Restore(
        SimulationRuntimeState runtime)
    {
        ArgumentNullException.ThrowIfNull(
            runtime);

        return new ApplyResolvedDamageAction(
            Resolution.Restore(
                runtime));
    }
}