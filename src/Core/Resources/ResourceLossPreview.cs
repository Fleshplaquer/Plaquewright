namespace Idler.Core.Resources;

public sealed class ResourceLossPreview
{
    internal ResourceState TargetState { get; }

    internal ulong ExpectedRevision { get; }

    public ResourceLossResult Result { get; }

    public double CurrentBefore { get; }

    public double CurrentAfter { get; }

    public double Maximum { get; }

    internal ResourceLossPreview(
        ResourceState targetState,
        ulong expectedRevision,
        ResourceLossResult result,
        double currentBefore,
        double currentAfter,
        double maximum)
    {
        ArgumentNullException.ThrowIfNull(
            targetState);

        TargetState = targetState;
        ExpectedRevision = expectedRevision;
        Result = result;
        CurrentBefore = currentBefore;
        CurrentAfter = currentAfter;
        Maximum = maximum;
    }
}