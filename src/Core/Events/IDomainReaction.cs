namespace Plaquewright.Core.Events;

public interface IDomainReaction<in TEvent, TWorkItem>
    where TEvent : IDomainEvent
{
    void React(
        TEvent domainEvent,
        DomainReactionContext<TWorkItem> context);
}