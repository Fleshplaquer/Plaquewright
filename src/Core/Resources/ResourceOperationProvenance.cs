using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Resources;

public readonly record struct ResourceOperationProvenance
{
    public ResourceOperationCause Cause { get; }

    public ExecutionId? GameplayExecutionId { get; }

    public bool HasGameplayExecution =>
        GameplayExecutionId.HasValue;

    public bool IsValid =>
        Enum.IsDefined(Cause) &&
        (int)Cause > 0 &&
        (!GameplayExecutionId.HasValue ||
         GameplayExecutionId.Value.IsValid);

    public ResourceOperationProvenance(
        ResourceOperationCause cause)
    {
        ValidateCause(
            cause);

        Cause =
            cause;

        GameplayExecutionId =
            null;
    }

    public ResourceOperationProvenance(
        ResourceOperationCause cause,
        ExecutionId gameplayExecutionId)
    {
        ValidateCause(
            cause);

        if (!gameplayExecutionId.IsValid)
        {
            throw new ArgumentException(
                "Gameplay execution ID must be valid.",
                nameof(gameplayExecutionId));
        }

        Cause =
            cause;

        GameplayExecutionId =
            gameplayExecutionId;
    }

    public override string ToString()
    {
        return IsValid
            ? Cause.ToString()
            : "<invalid>";
    }

    private static void ValidateCause(
        ResourceOperationCause cause)
    {
        if (!Enum.IsDefined(cause) ||
            (int)cause <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cause),
                cause,
                "Resource operation cause must be a known non-zero value.");
        }
    }
}