using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceOperationLedgerEntryAmountValidationTests
{
    [Fact]
    public void LossEntry_RequestAndResultAmountMismatch_IsRejected()
    {
        var resourceId =
            new ResourceId(1);

        var request =
            new ResourceLossRequest(
                resourceId,
                25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived));

        var result =
            new ResourceLossResult(
                resourceId,
                requestedLoss: 10d,
                preventedLoss: 0d,
                actualLoss: 10d);

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceLossLedgerEntry(
                    new EntityId(1UL),
                    request,
                    result));
    }

    [Fact]
    public void CostEntry_RequestAndResultAmountMismatch_IsRejected()
    {
        var resourceId =
            new ResourceId(1);

        var request =
            new ResourceCostRequest(
                resourceId,
                25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.SkillCost));

        var result =
            new ResourceCostResult(
                resourceId,
                requestedCost: 10d);

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceCostLedgerEntry(
                    new EntityId(1UL),
                    request,
                    result));
    }

    [Fact]
    public void RecoveryEntry_RequestAndResultAmountMismatch_IsRejected()
    {
        var resourceId =
            new ResourceId(1);

        var request =
            new ResourceRecoveryRequest(
                resourceId,
                25d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Recovery));

        var result =
            new ResourceRecoveryResult(
                resourceId,
                requestedRecovery: 10d,
                actualRecovery: 10d);

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceRecoveryLedgerEntry(
                    new EntityId(1UL),
                    request,
                    result));
    }
}