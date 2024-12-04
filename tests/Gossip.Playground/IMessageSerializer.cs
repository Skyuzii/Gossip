using Gossip.Core.Abstractions.Messages.Common;

namespace Gossip.Playground;

public interface IMessageSerializer
{
    byte[] Serialize(Message message);

    Message Deserialize(MessageType messageType, byte[] message);
}