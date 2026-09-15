namespace Plaquewright.Core.Transactions;

public static class TransactionCoordinator
{
    public static bool TryCommit(
        params ITransactionParticipant[] participants)
    {
        ArgumentNullException.ThrowIfNull(
            participants);

        return TryCommit(
            (IEnumerable<ITransactionParticipant>)participants);
    }

    public static bool TryCommit(
        IEnumerable<ITransactionParticipant> participants)
    {
        ArgumentNullException.ThrowIfNull(
            participants);

        var participantArray =
            participants.ToArray();
        if (participantArray.Length == 0)
        {
            throw new ArgumentException(
                "A transaction must contain at least one participant.",
                nameof(participants));
        }

        for (var index = 0;
             index < participantArray.Length;
             index++)
        {
            if (participantArray[index] is null)
            {
                throw new ArgumentException(
                    "Transaction participants must not contain null entries.",
                    nameof(participants));
            }
        }

        var preparedChanges =
            new PreparedTransactionChange[
                participantArray.Length];

        //
        // Phase 1:
        // PREPARE EVERYTHING.
        //
        // Nothing may become visible during this phase.
        //
        for (var index = 0;
             index < participantArray.Length;
             index++)
        {
            var participant =
                participantArray[index];

            var accepted =
                participant.TryPrepare(
                    out var preparedChange);

            if (!accepted)
            {
                return false;
            }

            if (preparedChange is null)
            {
                throw new InvalidOperationException(
                    "A transaction participant reported successful preparation without providing a prepared change.");
            }

            preparedChanges[index] =
                preparedChange;
        }

        var uniquePreparedChanges =
    new HashSet<PreparedTransactionChange>(
        ReferenceEqualityComparer.Instance);

        for (var index = 0;
             index < preparedChanges.Length;
             index++)
        {
            if (!uniquePreparedChanges.Add(
                    preparedChanges[index]))
            {
                throw new InvalidOperationException(
                    "The same prepared transaction change was returned by more than one participant.");
            }
        }

        //
        // Phase 2:
        // APPLY EVERYTHING.
        //
        // Normal gameplay validation must already be complete.
        //
        for (var index = 0;
             index < preparedChanges.Length;
             index++)
        {
            preparedChanges[index].Apply();
        }

        return true;
    }
}