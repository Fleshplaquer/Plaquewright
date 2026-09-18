using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Composition;

public sealed class SimulationComposition<TWorkItem>
{
    private readonly SimulationExecutionPlan<TWorkItem>
        _executionPlan;

    private readonly HashSet<Type>
        _providedContracts;

    internal SimulationComposition(
        int moduleCount,
        IEnumerable<Type> providedContracts,
        SimulationExecutionPlan<TWorkItem>
            executionPlan)
    {
        ArgumentNullException.ThrowIfNull(
            providedContracts);

        ArgumentNullException.ThrowIfNull(
            executionPlan);

        ModuleCount =
            moduleCount;

        _providedContracts =
            new HashSet<Type>(
                providedContracts);

        _executionPlan =
            executionPlan;
    }

    public int ModuleCount { get; }

    public int HandlerCount =>
        _executionPlan.HandlerCount;

    public bool Provides<TContract>()
        where TContract : ISimulationModuleContract
    {
        return _providedContracts.Contains(
            typeof(TContract));
    }

    public void Execute(
        SimulationEventContext<TWorkItem> context)
    {
        _executionPlan.Execute(
            context);
    }
}