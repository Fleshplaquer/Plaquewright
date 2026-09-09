namespace Idler.Core.Simulation;

public static class DeterministicRngFactory
{
    public static DeterministicRng CreateStream(
        SimulationSeed rootSeed,
        RngDomainKey domain,
        RngAlgorithmVersion algorithmVersion =
            RngAlgorithmVersion.SplitMix64V1)
    {
        if (string.IsNullOrEmpty(domain.Value))
        {
            throw new ArgumentException(
                "RNG domain must be valid.",
                nameof(domain));
        }

        var domainHash =
            StableHash64.Compute(domain.Value);

        var initialState =
            rootSeed.Value ^ domainHash;

        return new DeterministicRng(
            initialState,
            algorithmVersion);
    }
}