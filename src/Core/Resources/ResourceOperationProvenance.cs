namespace Plaquewright.Core.Resources;

public readonly record struct ResourceOperationProvenance
{
    public ResourceOperationCause Cause { get; }

    public bool IsValid =>
        Enum.IsDefined(Cause) &&
        (int)Cause > 0;

    public ResourceOperationProvenance(
        ResourceOperationCause cause)
    {
        if (!Enum.IsDefined(cause) ||
            (int)cause <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(cause),
                cause,
                "Resource operation cause must be a known non-zero value.");
        }

        Cause = cause;
    }

    public override string ToString()
    {
        return IsValid
            ? Cause.ToString()
            : "<invalid>";
    }
}