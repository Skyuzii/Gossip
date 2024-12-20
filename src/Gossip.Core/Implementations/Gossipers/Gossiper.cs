using Gossip.Core.Abstractions;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigest;
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Implementations.Messages.Dispatchers;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Gossipers;

internal sealed class Gossiper : IGossiper
{
    private readonly IPeerManager _peerManager;
    private readonly GossiperOptions _options;
    private readonly ILogger<Gossiper> _logger;
    private readonly IMessageDispatcher _messageDispatcher;
    private readonly CancellationTokenSource _workLoopCancellation;
    private Task? _startSpreadingGossip;

    public Gossiper(
        IPeerManager peerManager,
        IMessageDispatcher messageDispatcher,
        GossiperOptions options,
        ILogger<Gossiper> logger)
    {
        _peerManager = peerManager;
        _messageDispatcher = messageDispatcher;
        _options = options;
        _logger = logger;
        _workLoopCancellation = new CancellationTokenSource();
    }

    public Task Start(CancellationToken cancellationToken)
    {
        return _startSpreadingGossip ??= StartSpreadingGossip(_workLoopCancellation.Token);
    }

    public Task Stop(CancellationToken cancellationToken)
    {
        Dispose();

        return _startSpreadingGossip ?? Task.CompletedTask;
    }

    public ValueTask Enqueue(Message message, CancellationToken cancellationToken)
    {
        return _messageDispatcher.Enqueue(message, cancellationToken);
    }

    private async Task StartSpreadingGossip(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(_options.SyncDigestInMs), cancellationToken);

                _logger.LogDebug("Start spreading gossip");

                await _messageDispatcher.Process(new StartRumorDigestMessage(_peerManager.LocalPeer.Address), cancellationToken);
            }
            catch (OperationCanceledException exception) when(exception.CancellationToken == cancellationToken)
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        _workLoopCancellation.Cancel();
        _workLoopCancellation.Dispose();
        _messageDispatcher.Dispose();
    }
}