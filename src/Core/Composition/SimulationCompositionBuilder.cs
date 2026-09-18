namespace Plaquewright.Core.Composition;

public sealed class SimulationCompositionBuilder<TWorkItem>
{
    private readonly List<
        SimulationModuleDefinition<TWorkItem>>
        _modules =
            new();

    public void AddModule(
        string name,
        Action<SimulationModuleBuilder<TWorkItem>>
            configure)
    {
        if (string.IsNullOrWhiteSpace(
                name))
        {
            throw new ArgumentException(
                "Module name cannot be empty.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(
            configure);

        foreach (var existing in _modules)
        {
            if (string.Equals(
                    existing.Name,
                    name,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Module '{name}' is already registered.");
            }
        }

        var moduleBuilder =
            new SimulationModuleBuilder<TWorkItem>(
                name);

        configure(
            moduleBuilder);

        _modules.Add(
            moduleBuilder.BuildDefinition());
    }

    public SimulationComposition<TWorkItem> Build()
    {
        var providers =
            new Dictionary<Type, string>();

        //
        // First pass:
        // establish the complete provider set.
        //
        foreach (var module in _modules)
        {
            foreach (var contractType in
                     module.ProvidedContracts)
            {
                if (!providers.TryAdd(
                        contractType,
                        module.Name))
                {
                    throw new InvalidOperationException(
                        $"Contract '{contractType.FullName}' is provided " +
                        $"by both module '{providers[contractType]}' " +
                        $"and module '{module.Name}'.");
                }
            }
        }

        //
        // Second pass:
        // every requirement must be satisfied before
        // any execution plan is produced.
        //
        foreach (var module in _modules)
        {
            foreach (var contractType in
                     module.RequiredContracts)
            {
                if (!providers.ContainsKey(
                        contractType))
                {
                    throw new InvalidOperationException(
                        $"Module '{module.Name}' requires contract " +
                        $"'{contractType.FullName}', but no module provides it.");
                }
            }
        }

        var executionPlanBuilder =
            new SimulationExecutionPlanBuilder<TWorkItem>();

        //
        // Module registration order is explicit and stable.
        // Dependencies validate availability; they do not
        // silently reorder modules.
        //
        foreach (var module in _modules)
        {
            module.AddHandlersTo(
                executionPlanBuilder);
        }

        return new SimulationComposition<TWorkItem>(
            _modules.Count,
            providers.Keys,
            executionPlanBuilder.Build());
    }
}