using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Abstractions.Messages.RumorDigestAck2;

public sealed record RumorDigestAck2Message(PeerAddress Sender, IReadOnlyCollection<FullPeerInfo> FullPeerInfos) : Message(MessageType.RumorDigestAck2, Sender);