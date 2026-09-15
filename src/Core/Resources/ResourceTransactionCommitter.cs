namespace Plaquewright.Core.Resources;

internal static class ResourceTransactionCommitter
{
    public static void Commit(
        ResourceTransactionDraft draft,
        ResourceOperationLedger ledger)
    {
        var preparedCommit =
            Prepare(
                draft,
                ledger);

        ApplyPrepared(
            preparedCommit);
    }

    internal static PreparedResourceTransactionCommit Prepare(
        ResourceTransactionDraft draft,
        ResourceOperationLedger ledger)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            ledger);

        // 1. Materialize all accepted gross operations.
        // May allocate, but mutates no gameplay state.
        var entries =
            ResourceTransactionLedgerEntryFactory.CreateEntries(
                draft);

        // 2. Validate every original ResourceState.
        // Any stale state or invalid final value aborts here.
        ResourceTransactionStateCommitter.Validate(
            draft);

        // 3. Validate/copy entries and reserve all ledger
        // capacity before touching gameplay state.
        var preparedLedgerAppend =
            ledger.PrepareAppendRange(
                entries);

        return new PreparedResourceTransactionCommit(
            draft,
            ledger,
            preparedLedgerAppend);
    }

    internal static void ApplyPrepared(
        PreparedResourceTransactionCommit preparedCommit)
    {
        ArgumentNullException.ThrowIfNull(
            preparedCommit);

        // From here onward there must be no ordinary
        // gameplay validation, allocation or callbacks.

        // 4. Publish final projected resource states.
        ResourceTransactionStateCommitter.ApplyValidated(
            preparedCommit.Draft);

        // 5. Publish accepted causal gross operations.
        preparedCommit.Ledger.ApplyPreparedAppend(
            preparedCommit.PreparedLedgerAppend);
    }
}