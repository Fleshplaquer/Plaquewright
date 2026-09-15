namespace Plaquewright.Core.Resources;

internal sealed class PreparedResourceLedgerAppend
{
    internal ResourceOperationLedger Ledger { get; }

    internal int ExpectedEntryCount { get; }

    internal ResourceOperationLedgerEntry[] Entries { get; }

    internal PreparedResourceLedgerAppend(
        ResourceOperationLedger ledger,
        int expectedEntryCount,
        ResourceOperationLedgerEntry[] entries)
    {
        ArgumentNullException.ThrowIfNull(
            ledger);

        ArgumentNullException.ThrowIfNull(
            entries);

        Ledger = ledger;
        ExpectedEntryCount = expectedEntryCount;
        Entries = entries;
    }
}