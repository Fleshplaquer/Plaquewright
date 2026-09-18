using Plaquewright.Core.Simulation;
using Plaquewright.Core.Transactions;

namespace Plaquewright.Core.Events;

public static class DomainEventTransactionCoordinator
{
    public static bool TryCommitAndPublish<TWorkItem, TEvent>(
        SimulationEventContext<TWorkItem> simulationContext,
        TEvent domainEvent,
        params ITransactionParticipant[] participants)
        where TEvent : TWorkItem, IDomainEvent
    {
        ArgumentNullException.ThrowIfNull(
            simulationContext);

        ArgumentNullException.ThrowIfNull(
            participants);

        if (domainEvent is null)
        {
            throw new ArgumentNullException(
                nameof(domainEvent));
        }

        using var preparedEvent =
            simulationContext.PrepareFollowUp();

        var committed =
            TransactionCoordinator.TryCommit(
                participants);

        if (!committed)
        {
            return false;
        }

        preparedEvent.Publish(
            domainEvent);

        return true;
    }
}