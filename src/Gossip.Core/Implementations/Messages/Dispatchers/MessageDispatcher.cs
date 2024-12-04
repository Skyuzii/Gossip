using System.Threading.Channels;

using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Implementations.Messages.Handlers;

namespace Gossip.Core.Implementations.Messages.Dispatchers;

internal sealed class MessageDispatcher : IMessageDispatcher
{
    private readonly Channel<Message> _channel;
    private readonly IReadOnlyDictionary<MessageType, IMessageHandler> _messageHandlers;
    private readonly CancellationTokenSource _workLoopCancellation;

    public MessageDispatcher(
        MessageDispatcherOptions options,
        IReadOnlyCollection<IMessageHandler> messageHandlers)
    {
        _messageHandlers = messageHandlers.ToDictionary(x => x.Type, y => y);
        _channel = Channel.CreateBounded<Message>(new BoundedChannelOptions(options.Capacity));

        _workLoopCancellation = new CancellationTokenSource();
        _ = StartProcessingInternal(_workLoopCancellation.Token);
    }

    public ValueTask Enqueue(Message message, CancellationToken cancellationToken)
    {
        return _channel.Writer.WriteAsync(message, cancellationToken);
    }

    private async Task StartProcessingInternal(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Message message = await _channel.Reader.ReadAsync(cancellationToken);

            if (!_messageHandlers.TryGetValue(message.Type, out IMessageHandler? messageHandler))
            {
                // todo: smth do
                throw new ArgumentOutOfRangeException(nameof(message.Type), message.Type, "Not found message handler for the type");
            }

            await messageHandler.Handle(message, cancellationToken);
        }
    }

    public void Dispose()
    {
        _channel.Writer.Complete();
        _workLoopCancellation.Cancel();
        _workLoopCancellation.Dispose();
    }
}