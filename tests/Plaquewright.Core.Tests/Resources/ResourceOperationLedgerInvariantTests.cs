using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

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
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    lifeId,
                    current: 100d,
                    maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                lifeId);

        var request =
            new ResourceLossRequest(
                lifeId,
                amount,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Direct));

        var preview =
            ResourceLossOperations.Preview(
                target.State,
                request);

        return ResourceLossOperations.Commit(
            target,
            preview);
    }
}