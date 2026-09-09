namespace Idler.Core.Simulation;

public readonly record struct SimulationRunResult
{
    public SimulationRunStatus Status { get; }

    public bool ResultComplete =>
        Status == SimulationRunStatus.Completed;

    public SimulationTime CurrentTime { get; }

    public ulong ProcessedEvents { get; }

    public int PendingEvents { get; }

    public SimulationBudgetKind? BudgetKind { get; }

    internal SimulationRunResult(
        SimulationRunStatus status,
        SimulationTime currentTime,
        ulong processedEvents,
        int pendingEvents,
        SimulationBudgetKind? budgetKind = null)
    {
        if (pendingEvents < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pendingEvents));
        }

        if (status ==
                SimulationRunStatus.BudgetExceeded &&
            budgetKind is null)
        {
            throw new ArgumentException(
                "A budget-exceeded result must specify the exceeded budget.",
                nameof(budgetKind));
        }

        if (status !=
                SimulationRunStatus.BudgetExceeded &&
            budgetKind is not null)
        {
            throw new ArgumentException(
                "Only a budget-exceeded result may specify an exceeded budget.",
                nameof(budgetKind));
        }

        Status = status;
        CurrentTime = currentTime;
        ProcessedEvents = processedEvents;
        PendingEvents = pendingEvents;
        BudgetKind = budgetKind;
    }
}