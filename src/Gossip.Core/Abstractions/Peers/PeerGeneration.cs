namespace Gossip.Core.Abstractions.Peers;

public readonly record struct PeerGeneration(long Value) : IComparable<PeerGeneration>
{
    public static PeerGeneration New()
    {
        return new PeerGeneration(Value: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    public static PeerGeneration Empty()
    {
        return new PeerGeneration(Value: 0);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public int CompareTo(PeerGeneration other)
    {
        return Value.CompareTo(other.Value);
    }

    public static bool operator ==(in PeerGeneration x, in PeerGeneration y)
    {
        return x.Equals(y);
    }

    public static bool operator !=(in PeerGeneration x, in PeerGeneration y)
    {
        return !x.Equals(y);
    }

    public static bool operator <(in PeerGeneration x, in PeerGeneration y)
    {
        return x.Value < y.Value;
    }

    public static bool operator <=(in PeerGeneration x, in PeerGeneration y)
    {
        return x.Value <= y.Value;
    }

    public static bool operator >(in PeerGeneration x, in PeerGeneration y)
    {
        return x.Value > y.Value;
    }

    public static bool operator >=(in PeerGeneration x, in PeerGeneration y)
    {
        return x.Value >= y.Value;
    }
}