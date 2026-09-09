using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceOperationLedgerInvariantTests
{
    [Fact]
    public void AppendingEntries_NeverReordersExistingEntries()
    {
        var ledger =
            new ResourceOperationLedger();

        var entries =
            Enumerable
                .Range(1, 10)
                .Select(CreateLossEntry)
                .ToArray();

        for (var index = 0;
             index < entries.Length;
             index++)
        {
            ledger.Append(
                entries[index]);

            Assert.Equal(
                index + 1,
                ledger.Count);

            for (var existingIndex = 0;
                 existingIndex <= index;
                 existingIndex++)
            {
                Assert.Same(
                    entries[existingIndex],
                    ledger.Entries[existingIndex]);
            }
        }
    }

    private static ResourceLossLedgerEntry CreateLossEntry(
        int amount)
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 100d,
                maximum: 100d);

        var request =
            new ResourceLossRequest(
                state.Id,
                amount,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Direct));

        var preview =
            ResourceLossOperations.Preview(
                state,
                request);

        return ResourceLossOperations.Commit(
            state,
            preview);
    }
}