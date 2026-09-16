using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

public sealed class PreDefeatInterventionContext
{
    internal ResourceTransactionDraft Draft { get; }

    public EntityRuntimeState Entity { get; }

    public ProjectedEntityDefeatEvaluation TriggerEvaluation { get; }

    /// <summary>
    /// The caller-selected causal execution for intervention operations.
    /// This is not inferred from the damage that triggered pre-defeat.
    /// Null denotes a context-free intervention.
    /// </summary>
    public ExecutionId? GameplayExecutionId { get; }

    public bool HasGameplayExecution =>
        GameplayExecutionId.HasValue;

    public ProjectedEntityDefeatObservation TriggerObservation =>
        TriggerEvaluation.Observation;

    public EntityId EntityId =>
        Entity.Id;

    public DefeatRelevantResourcePolicy Policy =>
        TriggerEvaluation.Policy;

    internal PreDefeatInterventionContext(
        ResourceTransactionDraft draft,
        EntityRuntimeState entity,
        ProjectedEntityDefeatEvaluation triggerEvaluation)
        : this(
            draft,
            entity,
            triggerEvaluation,
            gameplayExecutionId: null)
    {
    }

    internal PreDefeatInterventionContext(
        ResourceTransactionDraft draft,
        EntityRuntimeState entity,
        ProjectedEntityDefeatEvaluation triggerEvaluation,
        ExecutionId? gameplayExecutionId)
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            entity);

        ArgumentNullException.ThrowIfNull(
            triggerEvaluation);

        if (gameplayExecutionId.HasValue &&
            !gameplayExecutionId.Value.IsValid)
        {
            throw new ArgumentException(
                "Gameplay execution ID must be valid when provided.",
                nameof(gameplayExecutionId));
        }

        if (triggerEvaluation.EntityId !=
            entity.Id)
        {
            throw new InvalidOperationException(
                "Pre-defeat evaluation belongs to a different entity.");
        }

        if (!triggerEvaluation.IsNewDefeatTransition)
        {
            throw new InvalidOperationException(
                "Pre-defeat intervention context requires a new projected defeat transition.");
        }

        Draft =
            draft;

        Entity =
            entity;

        TriggerEvaluation =
            triggerEvaluation;

        GameplayExecutionId =
            gameplayExecutionId;
    }
}
