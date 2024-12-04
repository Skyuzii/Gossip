using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Abstractions.Messages.RumorDigest;

public sealed record RumorDigestMessage(PeerAddress Sender, IReadOnlyCollection<DigestPeerInfo> DigestPeerInfos) : Message(MessageType.RumorDigest, Sender);