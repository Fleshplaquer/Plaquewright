using Plaquewright.Core.Transactions;
using System.Diagnostics.CodeAnalysis;

namespace Plaquewright.Core.ExternalTests.Transactions;

public sealed class TransactionCoordinatorTests
{
    [Fact]
    public void TryCommit_PreparesAllParticipantsBeforeApplyingAny()
    {
        var events =
            new List<string>();

        var first =
            new TestParticipant(
                "first",
                events,
                accepts: true);

        var second =
            new TestParticipant(
                "second",
                events,
                accepts: true);

        var committed =
            TransactionCoordinator.TryCommit(
                first,
                second);

        Assert.True(
            committed);

        Assert.Equal(
            [
                "prepare:first",
                "prepare:second",
                "apply:first",
                "apply:second"
            ],
            events);
    }

    [Fact]
    public void TryCommit_WhenParticipantRejects_AppliesNothing()
    {
        var events =
            new List<string>();

        var first =
            new TestParticipant(
                "first",
                events,
                accepts: true);

        var second =
            new TestParticipant(
                "second",
                events,
                accepts: false);

        var committed =
            TransactionCoordinator.TryCommit(
                first,
                second);

        Assert.False(
            committed);

        Assert.Equal(
            [
                "prepare:first",
                "prepare:second"
            ],
            events);

        Assert.Equal(
            0,
            first.ApplyCount);

        Assert.Equal(
            0,
            second.ApplyCount);
    }

    [Fact]
    public void PreparedChange_HasNoPublicApplyMethod()
    {
        var publicApply =
            typeof(PreparedTransactionChange)
                .GetMethod(
                    "Apply",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);

        Assert.Null(
            publicApply);
    }
    [Fact]
    public void TryCommit_WithNoParticipants_Throws()
    {
        Assert.Throws<ArgumentException>(
            () =>
                TransactionCoordinator.TryCommit());
    }
    [Fact]
    public void TryCommit_WithNullParticipant_RejectsBeforePreparingAnything()
    {
        var events =
            new List<string>();

        var first =
            new TestParticipant(
                "first",
                events,
                accepts: true);

        var second =
            new TestParticipant(
                "second",
                events,
                accepts: true);

        ITransactionParticipant[] participants =
        [
            first,
        null!,
        second
        ];

        Assert.Throws<ArgumentException>(
            () =>
                TransactionCoordinator.TryCommit(
                    participants));

        Assert.Empty(
            events);

        Assert.Equal(
            0,
            first.PrepareCount);

        Assert.Equal(
            0,
            second.PrepareCount);

        Assert.Equal(
            0,
            first.ApplyCount);

        Assert.Equal(
            0,
            second.ApplyCount);
    }

    [Fact]
    public void TryCommit_WhenParticipantThrowsDuringPrepare_AppliesNothingAndStopsPreparing()
    {
        var events =
            new List<string>();

        var first =
            new TestParticipant(
                "first",
                events,
                accepts: true);

        var throwing =
            new ThrowingParticipant(
                events);

        var third =
            new TestParticipant(
                "third",
                events,
                accepts: true);

        var exception =
            Assert.Throws<InvalidOperationException>(
                () =>
                    TransactionCoordinator.TryCommit(
                        first,
                        throwing,
                        third));

        Assert.Equal(
            "prepare failed",
            exception.Message);

        Assert.Equal(
            [
                "prepare:first",
            "prepare:throwing"
            ],
            events);

        Assert.Equal(
            1,
            first.PrepareCount);

        Assert.Equal(
            0,
            first.ApplyCount);

        Assert.Equal(
            1,
            throwing.PrepareCount);

        Assert.Equal(
            0,
            third.PrepareCount);

        Assert.Equal(
            0,
            third.ApplyCount);
    }
    [Fact]
    public void TryCommit_WhenPreparedChangeIsReusedAcrossTransactions_RejectsSecondApplication()
    {
        var events =
            new List<string>();

        var sharedChange =
            new SharedPreparedChange(
                events);

        var participant =
            new SharedChangeParticipant(
                sharedChange);

        var firstCommit =
            TransactionCoordinator.TryCommit(
                participant);

        Assert.True(
            firstCommit);

        Assert.Equal(
            1,
            sharedChange.ApplyCount);

        Assert.Equal(
            [
                "apply:shared"
            ],
            events);

        Assert.Throws<InvalidOperationException>(
            () =>
                TransactionCoordinator.TryCommit(
                    participant));

        // The already-applied change must not run again.
        Assert.Equal(
            1,
            sharedChange.ApplyCount);

        Assert.Equal(
            [
                "apply:shared"
            ],
            events);
    }
    [Fact]
    public void TryCommit_WhenParticipantsReturnSamePreparedChange_RejectsBeforeApplyingAnything()
    {
        var events =
            new List<string>();

        var sharedChange =
            new SharedPreparedChange(
                events);

        var first =
            new SharedChangeParticipant(
                sharedChange);

        var second =
            new SharedChangeParticipant(
                sharedChange);

        Assert.Throws<InvalidOperationException>(
            () =>
                TransactionCoordinator.TryCommit(
                    first,
                    second));

        Assert.Empty(
            events);

        Assert.Equal(
            0,
            sharedChange.ApplyCount);
    }
    [Fact]
    public void TryCommit_WhenParticipantReportsSuccessWithoutPreparedChange_ThrowsBeforeApplyingAnything()
    {
        var events =
            new List<string>();

        var first =
            new TestParticipant(
                "first",
                events,
                accepts: true);

        var broken =
            new BrokenSuccessfulParticipant(
                events);

        Assert.Throws<InvalidOperationException>(
            () =>
                TransactionCoordinator.TryCommit(
                    first,
                    broken));

        Assert.Equal(
            [
                "prepare:first",
            "prepare:broken"
            ],
            events);

        Assert.Equal(
            1,
            first.PrepareCount);

        Assert.Equal(
            0,
            first.ApplyCount);

        Assert.Equal(
            1,
            broken.PrepareCount);
    }
    private sealed class SharedChangeParticipant
    : ITransactionParticipant
    {
        private readonly PreparedTransactionChange
            _preparedChange;

        public SharedChangeParticipant(
            PreparedTransactionChange preparedChange)
        {
            _preparedChange =
                preparedChange;
        }

        public bool TryPrepare(
            [NotNullWhen(true)]
        out PreparedTransactionChange? preparedChange)
        {
            preparedChange =
                _preparedChange;

            return true;
        }
    }

    private sealed class SharedPreparedChange
        : PreparedTransactionChange
    {
        private readonly List<string> _events;

        public int ApplyCount { get; private set; }

        public SharedPreparedChange(
            List<string> events)
        {
            _events =
                events;
        }

        protected override void ApplyCore()
        {
            ApplyCount++;

            _events.Add(
                "apply:shared");
        }
    }
    private sealed class ThrowingParticipant
    : ITransactionParticipant
    {
        private readonly List<string> _events;

        public int PrepareCount { get; private set; }

        public ThrowingParticipant(
            List<string> events)
        {
            _events =
                events;
        }

        public bool TryPrepare(
            [NotNullWhen(true)]
        out PreparedTransactionChange? preparedChange)
        {
            PrepareCount++;

            _events.Add(
                "prepare:throwing");

            preparedChange =
                null;

            throw new InvalidOperationException(
                "prepare failed");
        }
    }
    private sealed class BrokenSuccessfulParticipant
    : ITransactionParticipant
    {
        private readonly List<string> _events;

        public int PrepareCount { get; private set; }

        public BrokenSuccessfulParticipant(
            List<string> events)
        {
            _events =
                events;
        }

#pragma warning disable CS8762 // Intentionally violates the public nullability contract for runtime-guard testing.

        public bool TryPrepare(
            [NotNullWhen(true)]
    out PreparedTransactionChange? preparedChange)
        {
            PrepareCount++;

            _events.Add(
                "prepare:broken");

            preparedChange =
                null;

            return true;
        }

#pragma warning restore CS8762
    }

    private sealed class TestParticipant
        : ITransactionParticipant
    {
        private readonly string _name;
        private readonly List<string> _events;
        private readonly bool _accepts;

        public int ApplyCount { get; private set; }
        public int PrepareCount { get; private set; }


        public TestParticipant(
            string name,
            List<string> events,
            bool accepts)

        {

            _name =
                name;

            _events =
                events;

            _accepts =
                accepts;
        }

        public bool TryPrepare(
    [NotNullWhen(true)]
    out PreparedTransactionChange? preparedChange)
        {
            PrepareCount++;
            _events.Add(
                $"prepare:{_name}");

            if (!_accepts)
            {
                preparedChange =
                    null;

                return false;
            }

            preparedChange =
                new TestPreparedChange(
                    this,
                    _name,
                    _events);

            return true;
        }


        private sealed class TestPreparedChange
            : PreparedTransactionChange
        {
            private readonly TestParticipant _owner;
            private readonly string _name;
            private readonly List<string> _events;

            public TestPreparedChange(
                TestParticipant owner,
                string name,
                List<string> events)
            {
                _owner =
                    owner;

                _name =
                    name;

                _events =
                    events;
            }

            protected override void ApplyCore()
            {
                _events.Add(
                    $"apply:{_name}");

                _owner.ApplyCount++;
            }
        }
    }
}