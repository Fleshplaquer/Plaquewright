using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Composition;

public sealed class SimulationModuleBuilder<TWorkItem>
{
    private readonly string _name;

    private readonly HashSet<Type> _providedContracts =
        new();

    private readonly HashSet<Type> _requiredContracts =
        new();

    private readonly List<
        Action<SimulationExecutionPlanBuilder<TWorkItem>>>
        _handlerRegistrations =
            new();

    internal SimulationModuleBuilder(
        string name)
    {
        _name =
            name;
    }

    public string Name =>
        _name;

    public void Provides<TContract>()
        where TContract : ISimulationModuleContract
    {
        var contractType =
            typeof(TContract);

        if (_requiredContracts.Contains(
                contractType))
        {
            throw new InvalidOperationException(
                $"Module '{_name}' cannot both require and provide " +
                $"contract '{contractType.FullName}'.");
        }

        if (!_providedContracts.Add(
                contractType))
        {
            throw new InvalidOperationException(
                $"Module '{_name}' already provides contract " +
                $"'{contractType.FullName}'.");
        }
    }

    public void Requires<TContract>()
        where TContract : ISimulationModuleContract
    {
        var contractType =
            typeof(TContract);

        if (_providedContracts.Contains(
                contractType))
        {
            throw new InvalidOperationException(
                $"Module '{_name}' cannot both provide and require " +
                $"contract '{contractType.FullName}'.");
        }

        if (!_requiredContracts.Add(
                contractType))
        {
            throw new InvalidOperationException(
                $"Module '{_name}' already requires contract " +
                $"'{contractType.FullName}'.");
        }
    }

    public void Handle<TPayload>(
        Action<
            TPayload,
            SimulationEventContext<TWorkItem>> handler)
        where TPayload : TWorkItem
    {
        ArgumentNullException.ThrowIfNull(
            handler);

        _handlerRegistrations.Add(
            executionPlanBuilder =>
                executionPlanBuilder.Handle(
                    handler));
    }

    internal SimulationModuleDefinition<TWorkItem>
        BuildDefinition()
    {
        return new SimulationModuleDefinition<TWorkItem>(
            _name,
            _providedContracts.ToArray(),
            _requiredContracts.ToArray(),
            _handlerRegistrations.ToArray());
    }
}

internal sealed class SimulationModuleDefinition<TWorkItem>
{
    private readonly Action<
        SimulationExecutionPlanBuilder<TWorkItem>>[]
        _handlerRegistrations;

    internal SimulationModuleDefinition(
        string name,
        Type[] providedContracts,
        Type[] requiredContracts,
        Action<
            SimulationExecutionPlanBuilder<TWorkItem>>[]
            handlerRegistrations)
    {
        Name =
            name;

        ProvidedContracts =
            providedContracts;

        RequiredContracts =
            requiredContracts;

        _handlerRegistrations =
            handlerRegistrations;
    }

    internal string Name { get; }

    internal IReadOnlyList<Type> ProvidedContracts { get; }

    internal IReadOnlyList<Type> RequiredContracts { get; }

    internal void AddHandlersTo(
        SimulationExecutionPlanBuilder<TWorkItem>
            executionPlanBuilder)
    {
        ArgumentNullException.ThrowIfNull(
            executionPlanBuilder);

        foreach (var register in
                 _handlerRegistrations)
        {
            register(
                executionPlanBuilder);
        }
    }
}