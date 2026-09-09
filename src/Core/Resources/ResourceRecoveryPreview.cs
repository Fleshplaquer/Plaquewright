namespace Idler.Core.Resources;

public sealed class ResourceRecoveryPreview
{
    internal ResourceState TargetState { get; }

    internal ulong ExpectedRevision { get; }

    public ResourceRecoveryResult Result { get; }

    public double CurrentBefore { get; }

    public double CurrentAfter { get; }

    public double Maximum { get; }

    internal ResourceRecoveryPreview(
        ResourceState targetState,
        ulong expectedRevision,
        ResourceRecoveryResult result,
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