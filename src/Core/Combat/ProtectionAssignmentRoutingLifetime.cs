using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

internal sealed class ProtectionAssignmentRoutingLifetime
{
    private ProtectionAssignmentRoutingState?
        _currentState;

    private ResourceTransactionDraft?
        _resourceTransactionDraft;

    public void Initialize(
        ProtectionAssignmentRoutingState initialState)
    {
        ArgumentNullException.ThrowIfNull(
            initialState);

        if (_currentState is not null)
        {
            throw new InvalidOperationException(
                "Protection assignment routing lifetime is already initialized.");
        }

        _currentState =
            initialState;
    }

    public void ValidateCurrent(
        ProtectionAssignmentRoutingState state)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        if (!ReferenceEquals(
                _currentState,
                state))
        {
            throw new InvalidOperationException(
                "Protection assignment routing state is stale.");
        }
    }

    public void ValidateResourceTransactionDraft(
        ProtectionAssignmentRoutingState state,
        ResourceTransactionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ValidateCurrent(
            state);

        if (_resourceTransactionDraft is not null &&
            !ReferenceEquals(
                _resourceTransactionDraft,
                draft))
        {
            throw new InvalidOperationException(
                "Protection assignment routing state is bound to a different resource transaction draft.");
        }
    }

    public void Advance(
        ProtectionAssignmentRoutingState currentState,
        ProtectionAssignmentRoutingState nextState)
    {
        ArgumentNullException.ThrowIfNull(
            nextState);

        ValidateCurrent(
            currentState);

        _currentState =
            nextState;
    }

    public void BindResourceTransactionDraft(
        ProtectionAssignmentRoutingState state,
        ResourceTransactionDraft draft)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ValidateCurrent(
            state);

        if (_resourceTransactionDraft is null)
        {
            _resourceTransactionDraft =
                draft;

            return;
        }

        if (!ReferenceEquals(
                _resourceTransactionDraft,
                draft))
        {
            throw new InvalidOperationException(
                "Protection assignment routing state is bound to a different resource transaction draft.");
        }
    }
}