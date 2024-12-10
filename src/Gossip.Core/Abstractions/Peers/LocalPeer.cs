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
            new[] { Rumor.NextHeartBeat() });
    }
}