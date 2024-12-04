using System.Collections.Concurrent;

using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Playground;

internal sealed class HttpMessageSender : IMessageSender, IDisposable
{
    private const string MessageType = "Gossip-Message-Type";
    private readonly ConcurrentDictionary<PeerAddress, HttpClient> _httpClients;
    private readonly IMessageSerializer _messageSerializer;

    public HttpMessageSender(IMessageSerializer messageSerializer)
    {
        _messageSerializer = messageSerializer;
        _httpClients = new ConcurrentDictionary<PeerAddress, HttpClient>();
    }

    public async Task Send(PeerAddress receiver, Message message, CancellationToken cancellationToken)
    {
        try
        {
            HttpClient httpClient = GetOrCreateHttpClient(receiver);

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, "/gossiper")
            {
                Headers = { { MessageType, message.Type.ToString() } },
                Content = new ByteArrayContent(_messageSerializer.Serialize(message))
            };

            await httpClient.SendAsync(httpRequestMessage, cancellationToken);
        }
        catch (Exception e)
        {
            // todo: return error
            Console.WriteLine(e);
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