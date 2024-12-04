using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;

namespace Gossip.Core.Abstractions.Messages.Common;

public sealed record FullPeerInfo(PeerAddress Address, IReadOnlyCollection<Rumor> Rumors);