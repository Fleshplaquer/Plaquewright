namespace Plaquewright.Core.Simulation;

public sealed class SimulationBudgetExceededException
    : Exception
{
    public SimulationBudgetKind Kind { get; }

    public SimulationBudgetExceededException(
        SimulationBudgetKind kind,
        string message)
        : base(message)
    {
        Kind = kind;
    }
}