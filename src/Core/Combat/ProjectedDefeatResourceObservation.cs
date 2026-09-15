using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

public sealed class ProjectedDefeatResourceObservation
{
    public ResourceStateTarget ResourceTarget { get; }

    public ResourceId ResourceId =>
        ResourceTarget.ResourceId;

    public double OriginalCurrent { get; }

    public double ProjectedCurrent { get; }

    public double ProjectedMaximum { get; }

    public bool IsProjected { get; }

    public bool IsProjectedDepleted =>
        ProjectedCurrent == 0d;

    public bool IsNewDepletionCandidate { get; }

    internal ProjectedDefeatResourceObservation(
        ResourceStateTarget resourceTarget,
        ProjectedResourceValues projectedValues,
        bool isNewDepletionCandidate)
    {
        ArgumentNullException.ThrowIfNull(
            resourceTarget);

        if (projectedValues.ResourceId !=
            resourceTarget.ResourceId)
        {
            throw new InvalidOperationException(
                "Projected defeat observation values belong to a different resource.");
        }

        ResourceTarget =
            resourceTarget;

        OriginalCurrent =
            resourceTarget.State.Current;

        ProjectedCurrent =
            projectedValues.Current;

        ProjectedMaximum =
            projectedValues.Maximum;

        IsProjected =
            projectedValues.IsProjected;

        IsNewDepletionCandidate =
            isNewDepletionCandidate;
    }
}