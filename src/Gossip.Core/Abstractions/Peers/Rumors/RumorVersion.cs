namespace Gossip.Core.Abstractions.Peers.Rumors;

public readonly record struct RumorVersion(long Value) : IComparable<RumorVersion>
{
    private static long Version;

    public static RumorVersion New()
    {
        return new RumorVersion(Interlocked.Increment(ref Version));
    }

    public static RumorVersion Empty()
    {
        return new RumorVersion(Value: 0);
    }

    public override string ToString()
    {
        return Value.ToString();
    }

    public int CompareTo(RumorVersion other)
    {
        return Value.CompareTo(other.Value);
    }

    public static bool operator ==(in RumorVersion x, in RumorVersion y)
    {
        return x.Equals(y);
    }

    public static bool operator !=(in RumorVersion x, in RumorVersion y)
    {
        return !x.Equals(y);
    }

    public static bool operator <(in RumorVersion x, in RumorVersion y)
    {
        return x.Value < y.Value;
    }

    public static bool operator <=(in RumorVersion x, in RumorVersion y)
    {
        return x.Value <= y.Value;
    }

    public static bool operator >(in RumorVersion x, in RumorVersion y)
    {
        return x.Value > y.Value;
    }

    public static bool operator >=(in RumorVersion x, in RumorVersion y)
    {
        return x.Value >= y.Value;
    }
}