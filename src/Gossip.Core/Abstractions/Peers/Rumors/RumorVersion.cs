namespace Gossip.Core.Abstractions.Peers.Rumors;

public readonly record struct RumorVersion(long Timestamp, ulong SequenceId) : IComparable<RumorVersion>
{
    public static RumorVersion New()
    {
        return new RumorVersion(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), ulong.MinValue);
    }

    public static RumorVersion Empty => new RumorVersion(Timestamp: 0, SequenceId: 0);

    public RumorVersion Increment()
    {
        return new RumorVersion(Timestamp, SequenceId + 1);
    }

    public bool Equals(RumorVersion other)
    {
        return other.Timestamp == Timestamp && other.SequenceId == SequenceId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Timestamp, SequenceId);
    }

    public int CompareTo(RumorVersion other)
    {
        int cmp = Timestamp.CompareTo(other.Timestamp);
        if (cmp == 0)
        {
            cmp = SequenceId.CompareTo(other.SequenceId);
        }

        return cmp;
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
        if (x.Timestamp < y.Timestamp)
        {
            return true;
        }

        if (x.Timestamp == y.Timestamp)
        {
            return x.SequenceId < y.SequenceId;
        }

        return false;
    }

    public static bool operator <=(in RumorVersion x, in RumorVersion y)
    {
        if (x.Timestamp < y.Timestamp)
        {
            return true;
        }

        if (x.Timestamp == y.Timestamp)
        {
            return x.SequenceId <= y.SequenceId;
        }

        return false;
    }

    public static bool operator >(in RumorVersion x, in RumorVersion y)
    {
        if (x.Timestamp > y.Timestamp)
        {
            return true;
        }

        if (x.Timestamp == y.Timestamp)
        {
            return x.SequenceId > y.SequenceId;
        }

        return false;
    }

    public static bool operator >=(in RumorVersion x, in RumorVersion y)
    {
        if (x.Timestamp > y.Timestamp)
        {
            return true;
        }

        if (x.Timestamp == y.Timestamp)
        {
            return x.SequenceId >= y.SequenceId;
        }

        return false;
    }

    public override string ToString()
    {
        return $"{Timestamp}:{SequenceId}";
    }
}