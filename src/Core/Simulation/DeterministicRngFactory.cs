namespace Idler.Core.Simulation;

public static class DeterministicRngFactory
{
    public static DeterministicRng CreateStream(
        SimulationSeed rootSeed,
        RngDomainKey domain,
        RngAlgorithmVersion algorithmVersion =
            RngAlgorithmVersion.SplitMix64V1)
    {
        ValidateDomain(
            domain);

        var domainHash =
            StableHash64.Compute(
                domain.Value);

        var initialState =
            rootSeed.Value ^ domainHash;

        return new DeterministicRng(
            initialState,
            algorithmVersion);
    }

    public static DeterministicRng CreateExecutionStream(
        SimulationSeed rootSeed,
        RngDomainKey domain,
        ExecutionId executionId,
        RngAlgorithmVersion algorithmVersion =
            RngAlgorithmVersion.SplitMix64V1)
    {
        ValidateDomain(
            domain);

        if (!executionId.IsValid)
        {
            throw new ArgumentException(
                "Execution ID must be valid.",
                nameof(executionId));
        }

        var domainHash =
            StableHash64.Compute(
                domain.Value);

        var initialState =
            StableHash64.Compute(
                "execution");

        initialState =
            StableHash64.AppendUInt64(
                initialState,
                rootSeed.Value);

        initialState =
            StableHash64.AppendUInt64(
                initialState,
                domainHash);

        initialState =
            StableHash64.AppendUInt64(
                initialState,
                executionId.Value);

        return new DeterministicRng(
            initialState,
            algorithmVersion);
    }

    private static void ValidateDomain(
        RngDomainKey domain)
    {
        if (string.IsNullOrEmpty(
                domain.Value))
        {
            throw new ArgumentException(
                "RNG domain must be valid.",
                nameof(domain));
        }
    }
}