using Idler.Core.Resources;

namespace Idler.Core.Combat;

internal static class DamageResourceTransactionStager
{
    public static ResourceLossPreview Stage(
        ResourceTransactionDraft draft,
        DamageResourceLossPlan plan,
        double preventedResourceLoss = 0d)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            plan);

        return draft.StageLoss(
            plan.TargetContext.ResourceTarget,
            plan.Request,
            preventedResourceLoss);
    }
}