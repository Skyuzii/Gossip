using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;

namespace Gossip.Core.Abstractions.Messages.Common;

public readonly record struct DigestPeerInfo(PeerAddress Address, RumorVersion MaxVersion);