using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Abstractions.Messages.Common;

public abstract record Message(MessageType Type, PeerAddress Sender);