using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Composition;

public sealed class SimulationExecutionPlanBuilder<TWorkItem>
{
    private readonly Dictionary<
        Type,
        Action<SimulationEventContext<TWorkItem>>> _handlers =
            new();

    public void Handle<TPayload>(
        Action<
            TPayload,
            SimulationEventContext<TWorkItem>> handler)
        where TPayload : TWorkItem
    {
        ArgumentNullException.ThrowIfNull(
            handler);

        var payloadType =
            typeof(TPayload);

        if (_handlers.ContainsKey(
                payloadType))
        {
            throw new InvalidOperationException(
                $"A handler for work item type " +
                $"'{payloadType.FullName}' is already registered.");
        }

        _handlers.Add(
            payloadType,
            context =>
            {
                if (context.Payload is not TPayload payload)
                {
                    throw new InvalidOperationException(
                        $"Execution plan route for " +
                        $"'{payloadType.FullName}' received an incompatible payload.");
                }

                handler(
                    payload,
                    context);
            });
    }

    public SimulationExecutionPlan<TWorkItem> Build()
    {
        return new SimulationExecutionPlan<TWorkItem>(
            _handlers);
    }
}