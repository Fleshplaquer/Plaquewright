using Plaquewright.Core.Entities;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

internal sealed class DamageApplicationOwnerPlan
{
    public EntityRuntimeState Entity { get; }

    public DefeatRelevantResourcePolicy Policy { get; }

    public ExecutionId? PreDefeatGameplayExecutionId { get; }

    private readonly Action<PreDefeatInterventionContext>?
        _applyPreDefeatInterventions;

    public DamageApplicationOwnerPlan(
        EntityRuntimeState entity,
        DefeatRelevantResourcePolicy policy,
        ExecutionId? preDefeatGameplayExecutionId = null,
        Action<PreDefeatInterventionContext>?
            applyPreDefeatInterventions = null)
    {
        ArgumentNullException.ThrowIfNull(
            entity);

        if (preDefeatGameplayExecutionId.HasValue &&
            !preDefeatGameplayExecutionId.Value.IsValid)
        {
            throw new ArgumentException(
                "Pre-defeat gameplay execution ID must be valid when provided.",
                nameof(preDefeatGameplayExecutionId));
        }

        Entity =
            entity;

        Policy =
            policy;

        PreDefeatGameplayExecutionId =
            preDefeatGameplayExecutionId;

        _applyPreDefeatInterventions =
            applyPreDefeatInterventions;
    }

    internal void ApplyPreDefeatInterventions(
        PreDefeatInterventionContext context)
    {
        ArgumentNullException.ThrowIfNull(
            context);

        _applyPreDefeatInterventions?.Invoke(
            context);
    }
}