using Gossip.Core.Abstractions.Peers;
using Gossip.IntegrationsTests.Framework.Extensions;

using Microsoft.Extensions.Hosting;

namespace Gossip.IntegrationsTests.Framework;

internal sealed class GossiperHost
{
    private readonly IHost _host;

    public IPeerManager PeerManager { get; }

    public GossiperHost(IHost host)
    {
        _host = host;
        PeerManager = host.GetPeerManager();
    }

    public Task Start()
    {
        return _host.StartAsync();
    }

    public Task Stop()
    {
        return _host.StopAsync();
    }
}