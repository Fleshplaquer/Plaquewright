namespace Plaquewright.Core.Resources;

public sealed class ResourceCostPreview
{
    internal ResourceState TargetState { get; }

    internal ulong ExpectedRevision { get; }

    public ResourceCostRequest Request { get; }

    public bool IsAffordable { get; }

    public double AvailableAmount { get; }

    public double Shortfall { get; }

    public double CurrentBefore { get; }

    public double CurrentAfter { get; }

    public double Maximum { get; }

    public bool IsRepresentable { get; }

    public bool IsPayable =>
        IsAffordable &&
        IsRepresentable;

    internal double ProjectedActualCost { get; }

    internal ResourceCostPreview(
        ResourceState targetState,
        ulong expectedRevision,
        ResourceCostRequest request,
        bool isAffordable,
        double availableAmount,
        double shortfall,
        double currentBefore,
        double currentAfter,
        bool isRepresentable,
double projectedActualCost,
        double maximum)
    {
        ArgumentNullException.ThrowIfNull(
            targetState);

        TargetState = targetState;
        ExpectedRevision = expectedRevision;
        Request = request;
        IsAffordable = isAffordable;
        AvailableAmount = availableAmount;
        Shortfall = shortfall;
        CurrentBefore = currentBefore;
        CurrentAfter = currentAfter;
        Maximum = maximum;
        IsRepresentable =
    isRepresentable;

        ProjectedActualCost =
            projectedActualCost;
    }
}