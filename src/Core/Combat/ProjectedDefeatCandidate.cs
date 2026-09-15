using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

public sealed class ProjectedDefeatCandidate
{
    public ResourceStateTarget ResourceTarget { get; }

    public EntityId EntityId =>
        ResourceTarget.EntityId;

    public ResourceId ResourceId =>
        ResourceTarget.ResourceId;

    public double OriginalCurrent { get; }

    public double ProjectedCurrent { get; }

    public double ProjectedMaximum { get; }

    internal ProjectedDefeatCandidate(
        ResourceStateTarget resourceTarget,
        double originalCurrent,
        ProjectedResourceValues projectedValues)
    {
        ArgumentNullException.ThrowIfNull(
            resourceTarget);

        if (projectedValues.ResourceId !=
            resourceTarget.ResourceId)
        {
            throw new InvalidOperationException(
                "Projected defeat candidate values belong to a different resource.");
        }

        if (originalCurrent <= 0d)
        {
            throw new InvalidOperationException(
                "Projected defeat candidate requires a resource that was previously above zero.");
        }

        if (projectedValues.Current != 0d)
        {
            throw new InvalidOperationException(
                "Projected defeat candidate requires the projected resource value to be zero.");
        }

        ResourceTarget =
            resourceTarget;

        OriginalCurrent =
            originalCurrent;

        ProjectedCurrent =
            projectedValues.Current;

        ProjectedMaximum =
            projectedValues.Maximum;
    }
}