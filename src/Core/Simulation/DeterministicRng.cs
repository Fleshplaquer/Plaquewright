namespace Idler.Core.Simulation;

public sealed class DeterministicRng
{
    private const ulong GoldenGamma =
        0x9E3779B97F4A7C15UL;

    private ulong _state;

    internal DeterministicRng(
        ulong initialState,
        RngAlgorithmVersion algorithmVersion)
    {
        _state = initialState;

        AlgorithmVersion =
            algorithmVersion;
    }

    public RngAlgorithmVersion AlgorithmVersion { get; }

    public ulong NextUInt64()
    {
        return AlgorithmVersion switch
        {
            RngAlgorithmVersion.SplitMix64V1 =>
                NextSplitMix64(),

            _ => throw new InvalidOperationException(
                $"Unsupported RNG algorithm version '{AlgorithmVersion}'.")
        };
    }

    public ulong NextUInt64(
        ulong exclusiveUpperBound)
    {
        if (exclusiveUpperBound == 0UL)
        {
            throw new ArgumentOutOfRangeException(
                nameof(exclusiveUpperBound),
                exclusiveUpperBound,
                "Upper bound must be greater than zero.");
        }

        // Rejection sampling avoids modulo bias.
        var threshold = unchecked(
            (0UL - exclusiveUpperBound)
            % exclusiveUpperBound);

        while (true)
        {
            var value =
                NextUInt64();

            if (value < threshold)
            {
                continue;
            }

            return value
                % exclusiveUpperBound;
        }
    }

    private ulong NextSplitMix64()
    {
        _state = unchecked(
            _state + GoldenGamma);

        var value = _state;

        value = unchecked(
            (value ^ (value >> 30))
            * 0xBF58476D1CE4E5B9UL);

        value = unchecked(
            (value ^ (value >> 27))
            * 0x94D049BB133111EBUL);

        return value
            ^ (value >> 31);
    }
}