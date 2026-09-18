using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

internal static class CombatWorkItemSnapshotCodec
{
    public static CombatWorkItemSnapshot Capture(
        ISimulationWorkItem workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);

        return workItem switch
        {
            ApplyResolvedDamageAction action =>
                new CombatWorkItemSnapshot.ResolvedDamage(
                    ApplyResolvedDamageActionSnapshot.Capture(action)),

            DamageCommittedEvent domainEvent =>
                new CombatWorkItemSnapshot.CommittedDamage(
                    DamageCommittedEventSnapshot.Capture(domainEvent)),

            _ => throw new NotSupportedException(
                $"Work item type '{workItem.GetType().FullName}' " +
                "is not supported by the Combat snapshot profile.")
        };
    }

    public static ISimulationWorkItem Restore(
        CombatWorkItemSnapshot snapshot,
        SimulationRuntimeState runtime)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(runtime);

        return snapshot switch
        {
            CombatWorkItemSnapshot.ResolvedDamage damage =>
                damage.Data.Restore(runtime),

            // Restore an existing fact, not a live resolution or a commit.
            CombatWorkItemSnapshot.CommittedDamage committed =>
                committed.Data.Restore(),

            _ => throw new NotSupportedException(
                $"Snapshot type '{snapshot.GetType().FullName}' " +
                "is not supported by the Combat snapshot profile.")
        };
    }
}