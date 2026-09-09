namespace Idler.Core.Resources;

public readonly record struct ProjectedResourceValues
{
    public ResourceId ResourceId { get; }

    public double Current { get; }

    public double Maximum { get; }

    public bool IsProjected { get; }

    internal ProjectedResourceValues(
        ResourceId resourceId,
        double current,
        double maximum,
        bool isProjected)
    {
        ResourceId =
            resourceId;

        Current =
            current;

        Maximum =
            maximum;

        IsProjected =
            isProjected;
    }
}