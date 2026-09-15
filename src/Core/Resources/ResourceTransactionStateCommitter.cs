namespace Plaquewright.Core.Resources;

internal static class ResourceTransactionStateCommitter
{
    public static void Commit(
        ResourceTransactionDraft draft)
    {
        Validate(
            draft);

        ApplyValidated(
            draft);
    }

    internal static void Validate(
        ResourceTransactionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        foreach (var projection in draft.Projections)
        {
            projection.ValidateCanCommitToOriginal();
        }
    }

    internal static void ApplyValidated(
        ResourceTransactionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        foreach (var projection in draft.Projections)
        {
            projection.ApplyValidatedToOriginal();
        }
    }
}