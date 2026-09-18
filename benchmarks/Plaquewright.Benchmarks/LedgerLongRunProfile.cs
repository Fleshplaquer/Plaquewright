using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Transactions;

namespace Plaquewright.Benchmarks;

internal sealed class LedgerLongRunProfile
    : ILoadProfile
{
    private const int CommitCount =
        50_000;

    public string Name =>
        "LedgerLongRun";

    public ProfileRunResult Run()
    {
        const double amountPerCommit =
            1d;

        var initialAmount =
            CommitCount *
            amountPerCommit *
            2d;

        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.benchmark"),
                    ResourceRole.CostSource)
            ]);

        var resourceId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.benchmark"));

        var entity =
            new EntityRuntimeState(
                new EntityId(
                    1UL),
                registry,
                [
                    new ResourceState(
                        resourceId,
                        current:
                            initialAmount,
                        maximum:
                            initialAmount)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                resourceId);

        var ledger =
            new ResourceOperationLedger();

        var request =
            new ResourceCostRequest(
                resourceId,
                amountPerCommit,
                new ResourceOperationProvenance(
                    ResourceOperationCause.Direct));

        for (var index = 0;
             index < CommitCount;
             index++)
        {
            var participant =
                new ResourceCostTransactionParticipant(
                    target,
                    request,
                    ledger);

            if (!TransactionCoordinator.TryCommit(
                    participant))
            {
                throw new InvalidOperationException(
                    $"Resource transaction unexpectedly rejected at commit {index}.");
            }
        }

        var expectedCurrent =
            initialAmount -
            CommitCount *
            amountPerCommit;

        if (target.State.Current !=
            expectedCurrent)
        {
            throw new InvalidOperationException(
                "Resource final value changed.");
        }

        if (target.State.Revision !=
            (ulong)CommitCount)
        {
            throw new InvalidOperationException(
                "Resource revision count changed.");
        }

        if (ledger.Count !=
            CommitCount)
        {
            throw new InvalidOperationException(
                "Ledger entry count changed.");
        }

        return new ProfileRunResult(
            LogicalOperations:
                CommitCount,
            PeakPendingEvents:
                0,
            LedgerEntries:
                ledger.Count,
            TraceEntries:
                0,
            RetainedRoot:
                new LedgerRetentionRoot(
                    target,
                    ledger));
    }

    private sealed record LedgerRetentionRoot(
        ResourceStateTarget Target,
        ResourceOperationLedger Ledger);
}