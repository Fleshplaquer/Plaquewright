namespace Plaquewright.Core.Simulation;

public sealed class SimulationRunner<TPayload>
{
    private readonly SimulationScheduler<TPayload> _scheduler;
    private readonly SimulationRunnerLimits _limits;

    private SimulationBudgetKind? _terminalBudgetKind;
    private bool _isFaulted;

    public SimulationTime CurrentTime { get; private set; } =
        SimulationTime.Zero;

    public ulong ProcessedEvents { get; private set; }

    public SimulationRunner(
        SimulationScheduler<TPayload> scheduler,
        SimulationRunnerLimits limits)
    {
        ArgumentNullException.ThrowIfNull(scheduler);
        ArgumentNullException.ThrowIfNull(limits);

        _scheduler = scheduler;
        _limits = limits;
    }

    public ScheduledEventKey ScheduleExternalInput(
        SimulationTime time,
        TPayload payload)
    {
        if (_isFaulted)
        {
            throw new InvalidOperationException(
                "Simulation runner is faulted and cannot accept external input.");
        }

        if (_terminalBudgetKind is not null)
        {
            throw new InvalidOperationException(
                "Simulation runner reached a terminal budget and cannot accept external input.");
        }

        if (time < CurrentTime)
        {
            throw new InvalidOperationException(
                $"Cannot schedule external input at {time} before current simulation time {CurrentTime}.");
        }

        return _scheduler.Schedule(
            time,
            SchedulerPhase.Execution,
            payload);
    }
    public SimulationRunResult RunNext(
        Action<SimulationEventContext<TPayload>> execute)
    {
        ArgumentNullException.ThrowIfNull(execute);

        if (_isFaulted)
        {
            throw new InvalidOperationException(
                "Simulation runner is faulted and cannot continue.");
        }

        if (_terminalBudgetKind is { } terminalBudgetKind)
        {
            return CreateBudgetExceededResult(
                terminalBudgetKind);
        }

        if (!_scheduler.TryPeek(
                out var nextEvent))
        {
            return CreateCompletedResult();
        }

        if (nextEvent.Key.Time <
            CurrentTime)
        {
            _isFaulted = true;

            throw new InvalidOperationException(
                $"Scheduler attempted to move simulation time backward " +
                $"from {CurrentTime} to {nextEvent.Key.Time}.");
        }

        if (ProcessedEvents >=
            _limits.MaxProcessedEvents)
        {
            return AbortForBudget(
                SimulationBudgetKind.ProcessedEvents);
        }

        var dequeued =
            _scheduler.TryDequeue(
                out var scheduledEvent);

        if (!dequeued)
        {
            _isFaulted = true;

            throw new InvalidOperationException(
                "Scheduler contained a peekable event that could not be dequeued.");
        }

        CurrentTime =
            scheduledEvent.Key.Time;

        var context =
            new SimulationEventContext<TPayload>(
                _scheduler,
                scheduledEvent);

        SimulationBudgetKind? budgetExceededKind =
            null;

        _scheduler.BeginEventExecution(
            scheduledEvent.Key);

        try
        {
            execute(context);
        }
        catch (SimulationBudgetExceededException exception)
        {
            budgetExceededKind =
                exception.Kind;
        }
        catch
        {
            _isFaulted = true;
            throw;
        }
        finally
        {
            _scheduler.EndEventExecution(
                scheduledEvent.Key);
        }

        if (budgetExceededKind is { } kind)
        {
            return AbortForBudget(
                kind);
        }

        ProcessedEvents =
            checked(ProcessedEvents + 1UL);

        if (_scheduler.IsEmpty)
        {
            return CreateCompletedResult();
        }

        if (ProcessedEvents >=
            _limits.MaxProcessedEvents)
        {
            return AbortForBudget(
                SimulationBudgetKind.ProcessedEvents);
        }

        return CreateInProgressResult();
    }


    public SimulationRunResult RunToCompletion(
        Action<SimulationEventContext<TPayload>> execute)
    {
        ArgumentNullException.ThrowIfNull(execute);

        while (true)
        {
            var result =
                RunNext(execute);

            if (result.Status !=
                SimulationRunStatus.InProgress)
            {
                return result;
            }
        }
    }

    private SimulationRunResult AbortForBudget(
        SimulationBudgetKind kind)
    {
        _terminalBudgetKind = kind;

        return CreateBudgetExceededResult(
            kind);
    }

    private SimulationRunResult CreateInProgressResult()
    {
        return new SimulationRunResult(
            SimulationRunStatus.InProgress,
            CurrentTime,
            ProcessedEvents,
            _scheduler.Count);
    }

    private SimulationRunResult CreateCompletedResult()
    {
        return new SimulationRunResult(
            SimulationRunStatus.Completed,
            CurrentTime,
            ProcessedEvents,
            _scheduler.Count);
    }

    private SimulationRunResult CreateBudgetExceededResult(
        SimulationBudgetKind kind)
    {
        return new SimulationRunResult(
            SimulationRunStatus.BudgetExceeded,
            CurrentTime,
            ProcessedEvents,
            _scheduler.Count,
            kind);
    }
}