namespace Plaquewright.Core.Combat;

internal abstract class CombatWorkItemSnapshot
{
    // This in-memory profile has exactly the cases declared below.
    private CombatWorkItemSnapshot()
    {
    }

    internal sealed class ResolvedDamage : CombatWorkItemSnapshot
    {
        public ApplyResolvedDamageActionSnapshot Data { get; }

        internal ResolvedDamage(
            ApplyResolvedDamageActionSnapshot data)
        {
            ArgumentNullException.ThrowIfNull(data);
            Data = data;
        }
    }

    internal sealed class CommittedDamage : CombatWorkItemSnapshot
    {
        public DamageCommittedEventSnapshot Data { get; }

        internal CommittedDamage(
            DamageCommittedEventSnapshot data)
        {
            ArgumentNullException.ThrowIfNull(data);
            Data = data;
        }
    }
}