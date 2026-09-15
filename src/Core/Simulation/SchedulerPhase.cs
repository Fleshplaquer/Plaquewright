namespace Plaquewright.Core.Simulation;

public enum SchedulerPhase : byte
{
    StateBoundary = 0,
    Execution = 1,
    FollowUp = 2
}