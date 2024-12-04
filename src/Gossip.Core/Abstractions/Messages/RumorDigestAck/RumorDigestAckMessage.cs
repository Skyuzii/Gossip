using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Abstractions.Messages.RumorDigestAck;

public sealed record RumorDigestAckMessage(
    PeerAddress Sender,
    IReadOnlyCollection<DigestPeerInfo> DigestPeerInfos,
    IReadOnlyCollection<FullPeerInfo> FullPeerInfos) : Message(MessageType.RumorDigestAck, Sender);