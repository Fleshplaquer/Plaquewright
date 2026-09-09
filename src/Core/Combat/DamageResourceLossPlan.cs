using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Combat;

public sealed class DamageResourceLossPlan
{
    public DamageResourceTargetContext TargetContext { get; }

    public ResourceLossRequest Request { get; }

    public DamageTaken DamageTaken =>
        TargetContext.Resolution.Quantities.Taken;

    public EntityId TargetEntityId =>
        TargetContext.Resolution.TargetEntityId;

    public ResourceId ResourceId =>
        TargetContext.ResourceTarget.ResourceId;

    public double RequestedResourceLoss =>
        Request.Amount;

    internal DamageResourceLossPlan(
        DamageResourceTargetContext targetContext,
        double requestedResourceLoss)
    {
        ArgumentNullException.ThrowIfNull(
            targetContext);

        ValidateRequestedResourceLoss(
            requestedResourceLoss);

        TargetContext =
            targetContext;

        Request =
            new ResourceLossRequest(
                targetContext.ResourceTarget.ResourceId,
                requestedResourceLoss,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived));
    }

    private static void ValidateRequestedResourceLoss(
        double requestedResourceLoss)
    {
        if (!double.IsFinite(
                requestedResourceLoss))
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedResourceLoss),
                requestedResourceLoss,
                "Requested resource loss must be finite.");
        }

        if (requestedResourceLoss < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedResourceLoss),
                requestedResourceLoss,
                "Requested resource loss cannot be negative.");
        }
    }
}