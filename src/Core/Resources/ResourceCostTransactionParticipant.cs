using System.Diagnostics.CodeAnalysis;
using Plaquewright.Core.Transactions;

namespace Plaquewright.Core.Resources;

public sealed class ResourceCostTransactionParticipant
    : ITransactionParticipant
{
    private readonly ResourceStateTarget _target;

    private readonly ResourceCostRequest _request;

    private readonly ResourceOperationLedger _ledger;

    public ResourceCostTransactionParticipant(
        ResourceStateTarget target,
        ResourceCostRequest request,
        ResourceOperationLedger ledger)
    {
        ArgumentNullException.ThrowIfNull(
            target);

        ArgumentNullException.ThrowIfNull(
            ledger);

        if (target.ResourceId !=
            request.ResourceId)
        {
            throw new ArgumentException(
                "Resource cost request targets a different resource.",
                nameof(request));
        }

        _target =
            target;

        _request =
            request;

        _ledger =
            ledger;
    }

    public bool TryPrepare(
        [NotNullWhen(true)]
        out PreparedTransactionChange? preparedChange)
    {
        //
        // First determine whether this expected gameplay
        // rejection is affordable.
        //
        // This does not mutate gameplay state.
        //
        var preview =
            ResourceCostOperations.Preview(
                _target.ResourceRegistry,
                _target.State,
                _request);

        if (!preview.IsPayable)
        {
            preparedChange =
                null;

            return false;
        }

        //
        // Reuse the existing ResourceTransaction machinery.
        //
        var draft =
            new ResourceTransactionDraft(
                _target.ResourceRegistry);

        draft.StageCost(
            _target,
            _request);

        var preparedCommit =
            ResourceTransactionCommitter.Prepare(
                draft,
                _ledger);

        preparedChange =
            new PreparedResourceCostChange(
                preparedCommit);

        return true;
    }

    private sealed class PreparedResourceCostChange
        : PreparedTransactionChange
    {
        private readonly PreparedResourceTransactionCommit
            _preparedCommit;

        public PreparedResourceCostChange(
            PreparedResourceTransactionCommit preparedCommit)
        {
            ArgumentNullException.ThrowIfNull(
                preparedCommit);

            _preparedCommit =
                preparedCommit;
        }

        protected override void ApplyCore()
        {
            ResourceTransactionCommitter.ApplyPrepared(
                _preparedCommit);
        }
    }
}