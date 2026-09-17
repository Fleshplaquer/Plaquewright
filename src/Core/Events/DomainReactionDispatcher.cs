namespace Plaquewright.Core.Events;

public sealed class DomainReactionDispatcher<
    TEvent,
    TWorkItem>
    where TEvent : IDomainEvent
{
    private readonly IDomainReaction<
        TEvent,
        TWorkItem>[] _reactions;

    public DomainReactionDispatcher(
        params IDomainReaction<
            TEvent,
            TWorkItem>[] reactions)
    {
        ArgumentNullException.ThrowIfNull(
            reactions);

        for (var index = 0;
             index < reactions.Length;
             index++)
        {
            if (reactions[index] is null)
            {
                throw new ArgumentException(
                    "Reaction collection cannot contain null entries.",
                    nameof(reactions));
            }
        }

        //
        // Freeze composition order at construction.
        //
        _reactions =
            (IDomainReaction<
                TEvent,
                TWorkItem>[])reactions.Clone();
    }

    public int Count =>
        _reactions.Length;

    public void Dispatch(
        TEvent domainEvent,
        DomainReactionContext<TWorkItem> context)
    {
        if (domainEvent is null)
        {
            throw new ArgumentNullException(
                nameof(domainEvent));
        }

        ArgumentNullException.ThrowIfNull(
            context);

        for (var index = 0;
             index < _reactions.Length;
             index++)
        {
            _reactions[index].React(
                domainEvent,
                context);
        }
    }
}