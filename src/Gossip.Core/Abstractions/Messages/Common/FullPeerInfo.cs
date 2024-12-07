using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;

namespace Gossip.Core.Abstractions.Messages.Common;

public sealed record FullPeerInfo(PeerAddress Address, PeerGeneration Generation, IReadOnlyCollection<Rumor> Rumors);