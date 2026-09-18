using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Transactions;

namespace Plaquewright.Benchmarks;

internal sealed class LedgerLongRunProfile
    : ILoadProfile
{
    private const int DefaultCommitCount =
        50_000;

    private readonly int _commitCount;

    private readonly string _name;

    public LedgerLongRunProfile()
        : this(
            DefaultCommitCount,
            "LedgerLongRun")
    {
    }

    internal LedgerLongRunProfile(
        int commitCount)
        : this(
            commitCount,
            $"LedgerLongRun[{commitCount}]")
    {
    }

    private LedgerLongRunProfile(
        int commitCount,
        string name)
    {
        if (commitCount <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(commitCount),
                commitCount,
                "Ledger commit count must be greater than zero.");
        }

        _commitCount =
            commitCount;

        _name =
            name;
    }

    public string Name =>
        _name;

    public ProfileRunResult Run()
    {
        const double amountPerCommit =
            1d;

        var initialAmount =
            _commitCount *
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
             index < _commitCount;
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
            _commitCount *
            amountPerCommit;

        if (target.State.Current !=
            expectedCurrent)
        {
            throw new InvalidOperationException(
                "Resource final value changed.");
        }

        if (target.State.Revision !=
            (ulong)_commitCount)
        {
            throw new InvalidOperationException(
                "Resource revision count changed.");
        }

        if (ledger.Count !=
            _commitCount)
        {
            throw new InvalidOperationException(
                "Ledger entry count changed.");
        }

        return new ProfileRunResult(
            LogicalOperations:
                _commitCount,
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