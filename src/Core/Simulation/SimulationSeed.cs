namespace Plaquewright.Core.Simulation;

public readonly record struct SimulationSeed
{
    public ulong Value { get; }

    public SimulationSeed(ulong value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}