using Plaquewright.Core.Events;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.ExternalTests.Events;

public sealed class DomainReactionDispatcherTests
{
    [Fact]
    public void Dispatch_UsesFrozenCompositionOrderAndPreservesFollowUpOrder()
    {
        var scheduler =
            new SimulationScheduler<TestWorkItem>(
                new SimulationSchedulerLimits(
                    maxQueueSize: 100,
                    maxSameTimestampWave: 10));

        var runner =
            new SimulationRunner<TestWorkItem>(
                scheduler,
                new SimulationRunnerLimits(
                    maxProcessedEvents: 10UL));

        var firstReaction =
            new ScheduleNamedFollowUpReaction(
                "first");

        var secondReaction =
            new ScheduleNamedFollowUpReaction(
                "second");

        IDomainReaction<
            TestDomainEvent,
            TestWorkItem>[] composition =
        [
            firstReaction,
            secondReaction
        ];

        var dispatcher =
            new DomainReactionDispatcher<
                TestDomainEvent,
                TestWorkItem>(
                composition);

        //
        // Mutating the caller's array after composition must
        // not alter the dispatcher.
        //
        composition[0] =
            new ScheduleNamedFollowUpReaction(
                "replacement");

        runner.ScheduleExternalInput(
            new SimulationTime(100L),
            new TestDomainEvent());

        var followUpTrace =
            new List<string>();

        var result =
            runner.RunToCompletion(
                context =>
                {
                    switch (context.Payload)
                    {
                        case TestDomainEvent domainEvent:
                            dispatcher.Dispatch(
                                domainEvent,
                                new DomainReactionContext<TestWorkItem>(
                                    context));
                            break;

                        case NamedFollowUp followUp:
                            followUpTrace.Add(
                                followUp.Name);
                            break;

                        default:
                            throw new InvalidOperationException(
                                "Unknown test work item.");
                    }
                });

        Assert.Equal(
            SimulationRunStatus.Completed,
            result.Status);

        Assert.Equal(
            2,
            dispatcher.Count);

        Assert.Equal(
            new[]
            {
                "first",
                "second"
            },
            followUpTrace);
    }

    private abstract class TestWorkItem
    {
    }

    private sealed class TestDomainEvent
        : TestWorkItem,
          IDomainEvent
    {
    }

    private sealed class NamedFollowUp
        : TestWorkItem
    {
        public string Name { get; }

        public NamedFollowUp(
            string name)
        {
            Name =
                name;
        }
    }

    private sealed class ScheduleNamedFollowUpReaction
        : IDomainReaction<
            TestDomainEvent,
            TestWorkItem>
    {
        private readonly string _name;

        public ScheduleNamedFollowUpReaction(
            string name)
        {
            _name =
                name;
        }

        public void React(
            TestDomainEvent domainEvent,
            DomainReactionContext<TestWorkItem> context)
        {
            ArgumentNullException.ThrowIfNull(
                domainEvent);

            ArgumentNullException.ThrowIfNull(
                context);

            context.ScheduleFollowUp(
                new NamedFollowUp(
                    _name));
        }
    }
}