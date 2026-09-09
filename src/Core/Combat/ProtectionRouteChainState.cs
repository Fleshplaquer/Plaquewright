namespace Idler.Core.Combat;

public sealed class ProtectionRouteChainState
{
    public double InitialDamage { get; }

    public double PrimaryPathDamage { get; }

    public double ContinueRoutingDamage { get; }

    public double FinancedDamage { get; }

    public int AppliedRouteCount { get; }

    public bool IsComplete =>
        ContinueRoutingDamage == 0d;

    public double AccountedDamage =>
        PrimaryPathDamage +
        ContinueRoutingDamage +
        FinancedDamage;

    private ProtectionRouteChainState(
        double initialDamage,
        double primaryPathDamage,
        double continueRoutingDamage,
        double financedDamage,
        int appliedRouteCount)
    {
        InitialDamage =
            NormalizeZero(
                initialDamage);

        PrimaryPathDamage =
            NormalizeZero(
                primaryPathDamage);

        ContinueRoutingDamage =
            NormalizeZero(
                continueRoutingDamage);

        FinancedDamage =
            NormalizeZero(
                financedDamage);

        AppliedRouteCount =
            appliedRouteCount;
    }

    internal static ProtectionRouteChainState Start(
        double primaryPathDamage,
        double protectionRoutingDamage)
    {
        ValidateDamage(
            primaryPathDamage,
            nameof(primaryPathDamage));

        ValidateDamage(
            protectionRoutingDamage,
            nameof(protectionRoutingDamage));

        var initialDamage =
            primaryPathDamage +
            protectionRoutingDamage;

        if (!double.IsFinite(
                initialDamage))
        {
            throw new OverflowException(
                "Protection route chain initial damage exceeds the finite numeric range.");
        }

        return new ProtectionRouteChainState(
            initialDamage,
            primaryPathDamage,
            protectionRoutingDamage,
            financedDamage: 0d,
            appliedRouteCount: 0);
    }

    internal ProtectionRouteChainState Apply(
        ProtectionShortfallRoutingResult routing)
    {
        ArgumentNullException.ThrowIfNull(
            routing);

        if (routing.Financing.AssignedDamage !=
            ContinueRoutingDamage)
        {
            throw new InvalidOperationException(
                "Protection route must consume exactly the damage currently available for continued protection routing.");
        }

        var primaryPathDamage =
            PrimaryPathDamage +
            routing.SpillBackDamage;

        if (!double.IsFinite(
                primaryPathDamage))
        {
            throw new OverflowException(
                "Protection route spillback exceeds the finite numeric range.");
        }

        var financedDamage =
            FinancedDamage +
            routing.Financing.FinancedDamage;

        if (!double.IsFinite(
                financedDamage))
        {
            throw new OverflowException(
                "Accumulated financed damage exceeds the finite numeric range.");
        }

        var appliedRouteCount =
            checked(
                AppliedRouteCount + 1);

        return new ProtectionRouteChainState(
            InitialDamage,
            primaryPathDamage,
            routing.ContinueRoutingDamage,
            financedDamage,
            appliedRouteCount);
    }

    internal ProtectionRouteChainState FinalizeUnresolvedToPrimaryPath()
    {
        if (ContinueRoutingDamage == 0d)
        {
            return this;
        }

        var primaryPathDamage =
            PrimaryPathDamage +
            ContinueRoutingDamage;

        if (!double.IsFinite(
                primaryPathDamage))
        {
            throw new OverflowException(
                "Final protection routing spillback exceeds the finite numeric range.");
        }

        return new ProtectionRouteChainState(
            InitialDamage,
            primaryPathDamage,
            continueRoutingDamage: 0d,
            FinancedDamage,
            AppliedRouteCount);
    }

    private static void ValidateDamage(
        double amount,
        string parameterName)
    {
        if (!double.IsFinite(
                amount))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                amount,
                "Damage amount must be finite.");
        }

        if (amount < 0d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                amount,
                "Damage amount cannot be negative.");
        }
    }

    private static double NormalizeZero(
        double value)
    {
        return value == 0d
            ? 0d
            : value;
    }
}