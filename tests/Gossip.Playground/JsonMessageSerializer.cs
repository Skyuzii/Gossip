using System.Text.Json;

using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigest;
using Gossip.Core.Abstractions.Messages.RumorDigestAck;
using Gossip.Core.Abstractions.Messages.RumorDigestAck2;

namespace Gossip.Playground;

internal sealed class JsonMessageSerializer : IMessageSerializer
{
    public byte[] Serialize(Message message)
    {
        return message switch
        {
            RumorDigestMessage rumorDigestMessage => JsonSerializer.SerializeToUtf8Bytes(rumorDigestMessage),
            RumorDigestAckMessage rumorDigestAckMessage => JsonSerializer.SerializeToUtf8Bytes(rumorDigestAckMessage),
            RumorDigestAck2Message rumorDigestAck2Message => JsonSerializer.SerializeToUtf8Bytes(rumorDigestAck2Message),
            _ => throw new ArgumentOutOfRangeException(nameof(message))
        };
    }

    public Message Deserialize(MessageType messageType, byte[] message)
    {
        return messageType switch
        {
            MessageType.RumorDigest => JsonSerializer.Deserialize<RumorDigestMessage>(message)!,
            MessageType.RumorDigestAck => JsonSerializer.Deserialize<RumorDigestAckMessage>(message)!,
            MessageType.RumorDigestAck2 => JsonSerializer.Deserialize<RumorDigestAck2Message>(message)!,
            _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, message: null)
        };
    }
}