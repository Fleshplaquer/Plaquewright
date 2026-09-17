using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Events;

public sealed class DomainReactionContext<TWorkItem>
{
    private readonly SimulationEventContext<TWorkItem>
        _simulationContext;

    public DomainReactionContext(
        SimulationEventContext<TWorkItem> simulationContext)
    {
        ArgumentNullException.ThrowIfNull(
            simulationContext);

        _simulationContext =
            simulationContext;
    }

    public SimulationTime CurrentTime =>
        _simulationContext.CurrentTime;

    public ScheduledEventKey ScheduleFollowUp(
        TWorkItem workItem)
    {
        return _simulationContext.Schedule(
            CurrentTime,
            SchedulerPhase.FollowUp,
            workItem);
    }

    public ScheduledEventKey ScheduleFollowUp(
        SimulationTime time,
        TWorkItem workItem)
    {
        return _simulationContext.Schedule(
            time,
            SchedulerPhase.FollowUp,
            workItem);
    }
}