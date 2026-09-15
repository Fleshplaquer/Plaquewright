using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Idler.Core.Resources;

public sealed class ResourceOperationLedger
{
    private readonly List<ResourceOperationLedgerEntry>
        _entries = [];

    private readonly ReadOnlyCollection<ResourceOperationLedgerEntry>
        _readOnlyEntries;

    public ResourceOperationLedger()
    {
        _readOnlyEntries =
            _entries.AsReadOnly();
    }

    public int Count =>
        _entries.Count;

    public IReadOnlyList<ResourceOperationLedgerEntry> Entries =>
        _readOnlyEntries;

    internal void Append(
    ResourceOperationLedgerEntry entry)
    {
        ArgumentNullException.ThrowIfNull(
            entry);

        _entries.Add(
            entry);
    }

    internal PreparedResourceLedgerAppend PrepareAppendRange(
        IReadOnlyList<ResourceOperationLedgerEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(
            entries);

        var copiedEntries =
            new ResourceOperationLedgerEntry[
                entries.Count];

        for (var index = 0;
             index < entries.Count;
             index++)
        {
            var entry =
                entries[index];

            if (entry is null)
            {
                throw new ArgumentException(
                    "Prepared ledger entries cannot contain null values.",
                    nameof(entries));
            }

            copiedEntries[index] =
                entry;
        }

        var requiredCapacity =
            checked(
                _entries.Count +
                copiedEntries.Length);

        // Any capacity allocation happens before
        // gameplay state is mutated.
        _entries.EnsureCapacity(
            requiredCapacity);

        return new PreparedResourceLedgerAppend(
            this,
            _entries.Count,
            copiedEntries);
    }

    internal void ApplyPreparedAppend(
        PreparedResourceLedgerAppend preparedAppend)
    {
        ArgumentNullException.ThrowIfNull(
            preparedAppend);

        // These are internal invariants. The final transaction
        // committer performs prepare + apply synchronously with
        // no callbacks between them.
        Debug.Assert(
            ReferenceEquals(
                preparedAppend.Ledger,
                this));

        Debug.Assert(
            _entries.Count ==
            preparedAppend.ExpectedEntryCount);

        Debug.Assert(
            _entries.Capacity >=
            _entries.Count +
            preparedAppend.Entries.Length);

        _entries.AddRange(
            preparedAppend.Entries);
    }
}