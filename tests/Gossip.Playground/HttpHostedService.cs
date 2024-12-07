using System.Text;
using System.Web;

using Gossip.Core.Abstractions;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;

using Microsoft.Extensions.Primitives;

namespace Gossip.Playground;

internal sealed class HttpHostedService : IHostedService
{
    private readonly IGossiper _gossiper;
    private readonly IPeerManager _peerManager;

    public HttpHostedService(IGossiper gossiper, IPeerManager peerManager)
    {
        _gossiper = gossiper;
        _peerManager = peerManager;
    }

    public async Task ProcessRequest(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HttpConstants.MessageTypeHeader, out StringValues messageTypeValues) || !Enum.TryParse(messageTypeValues.First(), out MessageType messageType))
        {
            return;
        }

        using var memoryStream = new MemoryStream();
        await context.Request.Body.CopyToAsync(memoryStream);

        Message message = JsonMessageSerializer.Deserialize(messageType, memoryStream.ToArray());

        await _gossiper.Enqueue(message, CancellationToken.None);
    }

    public Task ReturnPeers(HttpContext context)
    {
        StringBuilder peerInfo = new StringBuilder()
            .AppendLine("LocalPeer:")
            .AppendLine($"   Address: {_peerManager.LocalPeer.Address.Value}")
            .AppendLine($"   Generation: {_peerManager.LocalPeer.Generation.Value}")
            .AppendLine($"   Rumors:");

        foreach ((RumorName _, Rumor rumor) in _peerManager.LocalPeer.Rumors)
        {
            peerInfo
                .AppendLine($"      Name: {rumor.Name.Value}")
                .AppendLine($"      Value: {rumor.Value.Value}")
                .AppendLine($"      Version: {rumor.Version.ToString()}")
                .AppendLine(string.Empty);
        }

        foreach (RemotePeer remotePeer in _peerManager.ActiveRemotePeers)
        {
            peerInfo
                .AppendLine("RemotePeer:")
                .AppendLine($"   Address: {remotePeer.Address.Value}")
                .AppendLine($"   Generation: {remotePeer.Generation.Value}")
                .AppendLine($"   Rumors:");

            foreach ((RumorName _, Rumor rumor) in remotePeer.Rumors)
            {
                peerInfo
                    .AppendLine($"      Name: {rumor.Name.Value}")
                    .AppendLine($"      Value: {rumor.Value.Value}")
                    .AppendLine($"      Version: {rumor.Version.ToString()}")
                    .AppendLine(string.Empty);
            }
        }

        return context.Response.WriteAsync(peerInfo.ToString(), Encoding.UTF8);
    }

    public Task AddRumor(HttpContext context)
    {
        string rumorName = HttpUtility.ParseQueryString(context.Request.QueryString.Value).Get("rumorName");
        string rumorValue = HttpUtility.ParseQueryString(context.Request.QueryString.Value).Get("rumorValue");
        string? rumorVersionStr = HttpUtility.ParseQueryString(context.Request.QueryString.Value).Get("rumorVersion");

        RumorVersion? rumorVersion = null;

        if (!string.IsNullOrWhiteSpace(rumorVersionStr))
        {
            rumorVersion = new RumorVersion(long.Parse(rumorVersionStr));
        }

        _peerManager.LocalPeer.Apply(new[] { new Rumor(new RumorName(rumorName), new RumorValue(rumorValue), rumorVersion ?? RumorVersion.New()) });

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _gossiper.Start(cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _gossiper.Stop(cancellationToken);
    }
}