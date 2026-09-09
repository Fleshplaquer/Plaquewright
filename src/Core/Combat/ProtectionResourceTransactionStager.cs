using Idler.Core.Resources;

namespace Idler.Core.Combat;

internal static class ProtectionResourceTransactionStager
{
    public static ResourceBackedProtectionFinancingResult Stage(
        ResourceTransactionDraft draft,
        ResourceBackedProtectionFinancingPlan plan)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            plan);

        var preview =
            draft.StageLoss(
                plan.ResourceTarget,
                plan.ResourceRequest);

        return new ResourceBackedProtectionFinancingResult(
            plan,
            preview);
    }
}