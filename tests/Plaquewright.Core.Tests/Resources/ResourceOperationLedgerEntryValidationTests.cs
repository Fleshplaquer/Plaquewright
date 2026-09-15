using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceOperationLedgerEntryValidationTests
{
    [Fact]
    public void DefaultProvenance_IsRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new SyntheticLedgerEntry(
                    new EntityId(1UL),
                    new ResourceId(1),
                    default));
    }

    [Fact]
    public void ValidProvenance_IsAccepted()
    {
        var provenance =
            new ResourceOperationProvenance(
                ResourceOperationCause.DamageDerived);

        var entry =
            new SyntheticLedgerEntry(
                new EntityId(1UL),
                new ResourceId(1),
                provenance);

        Assert.Equal(
            provenance,
            entry.Provenance);

        Assert.Equal(
            new EntityId(1UL),
            entry.TargetEntityId);

        Assert.Equal(
            new ResourceId(1),
            entry.ResourceId);
    }

    private sealed class SyntheticLedgerEntry
        : ResourceOperationLedgerEntry
    {
        public SyntheticLedgerEntry(
            EntityId targetEntityId,
            ResourceId resourceId,
            ResourceOperationProvenance provenance)
            : base(
                targetEntityId,
                resourceId,
                provenance)
        {
        }
    }
}