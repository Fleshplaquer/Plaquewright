using Idler.Core.Stats;

namespace Idler.Core.Combat;

public sealed class DamageRangeModifierAccumulator
{
    private readonly ModifierAccumulator _minimum = new();
    private readonly ModifierAccumulator _maximum = new();

    public void AddFlat(
        double value,
        DamageRangeTarget target = DamageRangeTarget.Both)
    {
        ApplyToTarget(
            target,
            accumulator => accumulator.AddFlat(value));
    }

    public void AddIncreased(
        double value,
        DamageRangeTarget target = DamageRangeTarget.Both)
    {
        ApplyToTarget(
            target,
            accumulator => accumulator.AddIncreased(value));
    }

    public void AddReduced(
        double value,
        DamageRangeTarget target = DamageRangeTarget.Both)
    {
        ApplyToTarget(
            target,
            accumulator => accumulator.AddReduced(value));
    }

    public void AddMore(
        double value,
        DamageRangeTarget target = DamageRangeTarget.Both)
    {
        ApplyToTarget(
            target,
            accumulator => accumulator.AddMore(value));
    }

    public void AddLess(
        double value,
        DamageRangeTarget target = DamageRangeTarget.Both)
    {
        ApplyToTarget(
            target,
            accumulator => accumulator.AddLess(value));
    }

    public DamageRange Apply(
        DamageRange baseRange,
        RangeResolutionPolicy policy =
            RangeResolutionPolicy.CollapseBetween)
    {
        var candidateMin =
            _minimum.Apply(baseRange.Min);

        var candidateMax =
            _maximum.Apply(baseRange.Max);

        return DamageRangeResolver.Resolve(
            candidateMin,
            candidateMax,
            policy);
    }

    private void ApplyToTarget(
        DamageRangeTarget target,
        Action<ModifierAccumulator> action)
    {
        switch (target)
        {
            case DamageRangeTarget.Both:
                action(_minimum);
                action(_maximum);
                break;

            case DamageRangeTarget.Minimum:
                action(_minimum);
                break;

            case DamageRangeTarget.Maximum:
                action(_maximum);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(target),
                    target,
                    "Unknown damage range target.");
        }
    }
}