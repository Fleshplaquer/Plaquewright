namespace Idler.Core.Simulation;

internal static class StableHash64
{
    private const ulong OffsetBasis =
        14695981039346656037UL;

    private const ulong Prime =
        1099511628211UL;

    public static ulong Compute(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var hash = OffsetBasis;

        foreach (var character in value)
        {
            // RngDomainKey currently allows only ASCII characters.
            hash ^= character;

            hash = unchecked(
                hash * Prime);
        }

        return hash;
    }

    public static ulong AppendUInt64(
        ulong hash,
        ulong value)
    {
        // Fixed little-endian byte order is part
        // of the deterministic replay contract.
        for (var shift = 0;
             shift < 64;
             shift += 8)
        {
            var currentByte =
                (byte)(value >> shift);

            hash ^= currentByte;

            hash = unchecked(
                hash * Prime);
        }

        return hash;
    }
}