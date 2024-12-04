using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Abstractions.Messages.RumorDigest;

public sealed record StartRumorDigestMessage(PeerAddress Sender) : Message(MessageType.StartRumorDigest, Sender);