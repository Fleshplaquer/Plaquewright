using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

public sealed class PreDefeatInterventionContext
{
    internal ResourceTransactionDraft Draft { get; }

    public EntityRuntimeState Entity { get; }

    public ProjectedEntityDefeatEvaluation TriggerEvaluation { get; }

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
    {
        ArgumentNullException.ThrowIfNull(
            draft);

        ArgumentNullException.ThrowIfNull(
            entity);

        ArgumentNullException.ThrowIfNull(
            triggerEvaluation);

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
    }
}