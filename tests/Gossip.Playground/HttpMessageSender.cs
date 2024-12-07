using System.Collections.Concurrent;

using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Playground;

internal sealed class HttpMessageSender : IMessageSender, IDisposable
{
    private const string MessageType = "Gossip-Message-Type";
    private readonly ConcurrentDictionary<PeerAddress, HttpClient> _httpClients;
    private readonly ILogger<HttpMessageSender> _logger;

    public HttpMessageSender(ILogger<HttpMessageSender> logger)
    {
        _logger = logger;
        _httpClients = new ConcurrentDictionary<PeerAddress, HttpClient>();
    }

    public async Task<MessageSendResult> Send(PeerAddress receiver, Message message, CancellationToken cancellationToken)
    {
        try
        {
            HttpClient httpClient = GetOrCreateHttpClient(receiver);

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/gossiper")
            {
                Headers = { { MessageType, message.Type.ToString() } },
                Content = new ByteArrayContent(JsonMessageSerializer.Serialize(message))
            };

            await httpClient.SendAsync(httpRequestMessage, cancellationToken);

            return MessageSendResult.Success;
        }
        catch (Exception exception)
        {
            _logger.LogError("Error send message -> {ExceptionMessage}", exception.Message);

            return MessageSendResult.Fail;
        }
    }

    private HttpClient GetOrCreateHttpClient(PeerAddress receiver)
    {
        return _httpClients.GetOrAdd(receiver, _ => new HttpClient(new HttpClientHandler())
        {
            BaseAddress = receiver.Value
        });
    }

    public void Dispose()
    {
        foreach (KeyValuePair<PeerAddress, HttpClient> httpClient in _httpClients)
        {
            httpClient.Value.Dispose();
        }
    }
}