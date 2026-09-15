namespace Plaquewright.Core.Resources;

public sealed class ResourceRecoveryPreview
{
    internal ResourceState TargetState { get; }

    internal ulong ExpectedRevision { get; }

    public ResourceRecoveryRequest Request { get; }

    public ResourceRecoveryResult Result { get; }

    public double CurrentBefore { get; }

    public double CurrentAfter { get; }

    public double Maximum { get; }

    internal ResourceRecoveryPreview(
        ResourceState targetState,
        ulong expectedRevision,
        ResourceRecoveryRequest request,
        ResourceRecoveryResult result,
        double currentBefore,
        double currentAfter,
        double maximum)
    {
        ArgumentNullException.ThrowIfNull(targetState);

        TargetState = targetState;
        ExpectedRevision = expectedRevision;
        Request = request;
        Result = result;
        CurrentBefore = currentBefore;
        CurrentAfter = currentAfter;
        Maximum = maximum;
    }
}