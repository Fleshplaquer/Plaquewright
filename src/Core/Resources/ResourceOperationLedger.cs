using System.Collections.ObjectModel;

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

    public void Append(
        ResourceOperationLedgerEntry entry)
    {
        ArgumentNullException.ThrowIfNull(
            entry);

        _entries.Add(
            entry);
    }
}