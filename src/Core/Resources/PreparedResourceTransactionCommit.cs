namespace Plaquewright.Core.Resources;

internal sealed class PreparedResourceTransactionCommit
{
    internal ResourceTransactionDraft Draft { get; }

    internal ResourceOperationLedger Ledger { get; }

    internal PreparedResourceLedgerAppend PreparedLedgerAppend { get; }

    internal PreparedResourceTransactionCommit(
        ResourceTransactionDraft draft,
        ResourceOperationLedger ledger,
        PreparedResourceLedgerAppend preparedLedgerAppend)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            ledger);

        ArgumentNullException.ThrowIfNull(
            preparedLedgerAppend);

        Draft =
            draft;

        Ledger =
            ledger;

        PreparedLedgerAppend =
            preparedLedgerAppend;
    }
}