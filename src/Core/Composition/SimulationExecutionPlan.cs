using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Composition;

public sealed class SimulationExecutionPlan<TWorkItem>
{
    private readonly IReadOnlyDictionary<
        Type,
        Action<SimulationEventContext<TWorkItem>>> _handlers;

    internal SimulationExecutionPlan(
        IReadOnlyDictionary<
            Type,
            Action<SimulationEventContext<TWorkItem>>> handlers)
    {
        ArgumentNullException.ThrowIfNull(
            handlers);

        _handlers =
            new Dictionary<
                Type,
                Action<SimulationEventContext<TWorkItem>>>(
                handlers);
    }

    public int HandlerCount =>
        _handlers.Count;

    public void Execute(
        SimulationEventContext<TWorkItem> context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        if (context.Payload is null)
        {
            throw new InvalidOperationException(
                "Simulation execution plan cannot dispatch a null work item.");
        }

        var payloadType =
            context.Payload.GetType();

        if (!_handlers.TryGetValue(
                payloadType,
                out var handler))
        {
            throw new InvalidOperationException(
                $"No simulation execution handler is registered for " +
                $"work item type '{payloadType.FullName}'.");
        }

        handler(
            context);
    }
}