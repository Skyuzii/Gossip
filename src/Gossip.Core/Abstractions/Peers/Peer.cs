using System.Collections.Concurrent;

using Gossip.Core.Abstractions.Peers.Rumors;

namespace Gossip.Core.Abstractions.Peers;

public abstract class Peer
{
    protected ConcurrentDictionary<RumorName, Rumor> RumorsProtected;

    public PeerAddress Address { get; }

    public PeerGeneration Generation { get; protected set; }

    public DateTimeOffset RumorsUpdatedAt { get; protected set; }

    public IReadOnlyDictionary<RumorName, Rumor> Rumors => RumorsProtected;

    protected Peer(
        PeerAddress address,
        PeerGeneration generation,
        IReadOnlyCollection<Rumor> rumors)
    {
        Address = address;
        Generation = generation;
        RumorsProtected = new ConcurrentDictionary<RumorName, Rumor>(rumors.ToDictionary(x => x.Name, y => y));
    }

    public RumorVersion GetMaxVersion()
    {
        return RumorsProtected.Values.MaxBy(x => x.Version)?.Version ?? RumorVersion.Empty();
    }

    protected void Apply(Rumor rumor)
    {
        if (!RumorsProtected.TryGetValue(rumor.Name, out Rumor? existRumor))
        {
            RumorsProtected.TryAdd(rumor.Name, rumor);
            RumorsUpdatedAt = DateTimeOffset.UtcNow;
            return;
        }

        if (rumor.Version > existRumor.Version)
        {
            RumorsProtected[rumor.Name] = rumor;
            RumorsUpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}