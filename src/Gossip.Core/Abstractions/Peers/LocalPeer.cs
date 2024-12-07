using Gossip.Core.Abstractions.Peers.Rumors;

namespace Gossip.Core.Abstractions.Peers;

public sealed class LocalPeer : Peer
{
    private LocalPeer(
        PeerAddress address,
        PeerGeneration generation,
        IReadOnlyCollection<Rumor> rumors) : base(address, generation, rumors)
    {
    }

    public static LocalPeer New(PeerAddress address)
    {
        return new LocalPeer(
            address,
            PeerGeneration.New(),
            new[] { new Rumor(RumorName.HeartBeat, new RumorValue("Good"), RumorVersion.New()) });
    }

    /// <summary>
    /// Apply rumors
    /// Add if none exists, or update if version is greater
    /// </summary>
    /// <param name="rumors"></param>
    public void Apply(IReadOnlyCollection<Rumor> rumors)
    {
        foreach (Rumor? rumor in rumors)
        {
            Apply(rumor);
        }
    }
}