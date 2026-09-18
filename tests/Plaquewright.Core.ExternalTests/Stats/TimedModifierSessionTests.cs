using Plaquewright.Core.Composition;
using Plaquewright.Core.Hosting;
using Plaquewright.Core.Simulation;
using Plaquewright.Core.Stats;

namespace Plaquewright.Core.ExternalTests.Stats;

public sealed class TimedModifierSessionTests
{
    [Fact]
    public void TimedModifier_ExpiresAtStateBoundaryBeforeSameTimeExecution()
    {
        var modifiers =
            new TimedModifierStateSet();

        var key =
            TimedModifierKey.Parse(
                "status.resistance_boost");

        var observations =
            new List<double>();

        var trace =
            new List<string>();

        var builder =
            new SimulationCompositionBuilder<
                TestWorkItem>();

        builder.AddModule(
            "TimedResistance",
            module =>
            {
                module.Handle<ApplyModifierInput>(
                    (input, context) =>
                    {
                        trace.Add(
                            nameof(ApplyModifierInput));

                        var expiration =
                            modifiers.ApplyOrRefresh(
                                key,
                                ModifierKind.Flat,
                                input.Amount,
                                context.CurrentTime,
                                input.Duration);

                        context.Schedule(
                            expiration.ExpiresAt,
                            SchedulerPhase.StateBoundary,
                            new ExpireModifierAction(
                                expiration));
                    });

                module.Handle<ExpireModifierAction>(
                    (action, context) =>
                    {
                        trace.Add(
                            nameof(ExpireModifierAction));

                        Assert.True(
                            modifiers.Expire(
                                action.Expiration,
                                context.CurrentTime));
                    });

                module.Handle<ObserveResistanceInput>(
                    (_, _) =>
                    {
                        trace.Add(
                            nameof(ObserveResistanceInput));

                        observations.Add(
                            TimedModifierValueQuery
                                .Evaluate(
                                    100d,
                                    modifiers));
                    });
            });

        var session =
            new SimulationSession<TestWorkItem>(
                builder.Build(),
                new SimulationSchedulerLimits(
                    maxQueueSize: 20,
                    maxSameTimestampWave: 10),
                new SimulationRunnerLimits(
                    maxProcessedEvents: 20UL));

        session.ScheduleExternalInput(
            new SimulationTime(10L),
            new ApplyModifierInput(
                25d,
                new SimulationDuration(100L)));

        session.ScheduleExternalInput(
            new SimulationTime(50L),
            new ObserveResistanceInput());

        //
        // Expiration is scheduled later by the t=10 handler,
        // but StateBoundary must still execute before this
        // Execution-phase input at t=110.
        //
        session.ScheduleExternalInput(
            new SimulationTime(110L),
            new ObserveResistanceInput());

        var result =
            session.RunToCompletion();

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.Equal(
            new[]
            {
                125d,
                100d
            },
            observations);

        Assert.Equal(
            new[]
            {
                nameof(ApplyModifierInput),
                nameof(ObserveResistanceInput),
                nameof(ExpireModifierAction),
                nameof(ObserveResistanceInput)
            },
            trace);

        Assert.False(
            modifiers.IsActive(
                key));

        Assert.Equal(
            4UL,
            session.ProcessedEvents);
    }

    private abstract record TestWorkItem
    : ISimulationWorkItem;

    private sealed record ApplyModifierInput(
        double Amount,
        SimulationDuration Duration)
        : TestWorkItem;

    private sealed record ExpireModifierAction(
        TimedModifierExpiration Expiration)
        : TestWorkItem;

    private sealed record ObserveResistanceInput
        : TestWorkItem;
}