using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Abstractions.Messages;

public interface IReceiversPicker
{
    IReadOnlyCollection<PeerAddress> Pick();
}