using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Peers;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Messages.Handlers;

public abstract class BaseMessageHandler<TMessage> : IMessageHandler where TMessage : Message
{
    protected readonly IMessageSender MessageSender;
    protected readonly IPeerManager PeerManager;
    protected readonly ILogger Logger;

    protected BaseMessageHandler(
        MessageType messageType,
        IMessageSender messageSender,
        IPeerManager peerManager,
        ILogger logger)
    {
        Type = messageType;
        MessageSender = messageSender;
        PeerManager = peerManager;
        Logger = logger;
    }

    public MessageType Type { get; }

    protected abstract Task HandleInternal(TMessage message, CancellationToken cancellationToken);

    public Task Handle(Message message, CancellationToken cancellationToken)
    {
        if (message is not TMessage castedMessage)
        {
            throw new ArgumentOutOfRangeException(nameof(message), message, $"Message is not {typeof(TMessage)}");
        }

        Logger.LogDebug("Start handling message with type {@MessageType} by sender {Sender}", message.Type, castedMessage.Sender.Value);

        return HandleInternal(castedMessage, cancellationToken);
    }

    protected async Task SendMessage(PeerAddress receiver, Message message, CancellationToken cancellationToken)
    {
        Logger.LogDebug("Send to {Receiver} the message with type {@MessageType}", receiver.Value, message.Type);

        MessageSendResult result = await MessageSender.Send(receiver, message, cancellationToken);

        if (result == MessageSendResult.Fail)
        {
            Logger.LogDebug("{Receiver} is dead", receiver.Value);
            PeerManager.Unreachable(receiver);
        }
    }
}