using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;
using Gossip.IntegrationsTests.Framework;
using Gossip.IntegrationsTests.Framework.Configurations;
using Gossip.IntegrationsTests.Framework.Extensions;

using Microsoft.Extensions.Hosting;

namespace Gossip.IntegrationsTests;

public sealed class GossiperTests
{
    [Fact]
    public async Task StartHost_WithRemoteStartingPeerAddresses_RemoteStartingPeerShouldDiscoveredNewPeer()
    {
        IHost firstHost = HostFactory.Create(port: 5169, new GossiperConfiguration { LocalPeer = "http://localhost:5169/" });
        await firstHost.StartAsync();

        IHost secondHost = HostFactory.Create(port: 5260, new GossiperConfiguration { LocalPeer = "http://localhost:5260/", RemoteStartingPeerAddresses = new[] { "http://localhost:5169/" } });
        await secondHost.StartAsync();

        Peer discoveredPeer = await firstHost.GetPeerManager().WaitForDiscoveredPeer();

        Assert.Equal(secondHost.GetPeerManager().LocalPeer.Address, discoveredPeer.Address);
    }

    [Fact]
    public async Task StopHost_WithRemoteStartingPeerAddresses_RemoteStartingPeerShouldUnreachablePeer()
    {
        IHost firstHost = HostFactory.Create(port: 5169, new GossiperConfiguration { LocalPeer = "http://localhost:5169/" });
        await firstHost.StartAsync();

        IHost secondHost = HostFactory.Create(port: 5260, new GossiperConfiguration { LocalPeer = "http://localhost:5260/", RemoteStartingPeerAddresses = new[] { "http://localhost:5169/" } });
        await secondHost.StartAsync();

        IPeerManager firstPeerManager = firstHost.GetPeerManager();

        await firstPeerManager.WaitForDiscoveredPeer();

        await secondHost.StopAsync();

        PeerAddress unreachablePeer = await firstPeerManager.WaitForUnreachablePeer();

        Assert.Equal(secondHost.GetPeerManager().LocalPeer.Address, unreachablePeer);
    }

    [Fact]
    public async Task StartHost_WithRemoteStartingPeerAddresses_SpreadsNewRumor()
    {
        IHost firstHost = HostFactory.Create(port: 5169, new GossiperConfiguration { LocalPeer = "http://localhost:5169/" });
        await firstHost.StartAsync();

        IHost secondHost = HostFactory.Create(port: 5260, new GossiperConfiguration { LocalPeer = "http://localhost:5260/", RemoteStartingPeerAddresses = new[] { "http://localhost:5169/" } });
        await secondHost.StartAsync();

        IPeerManager firstPeerManager = firstHost.GetPeerManager();

        await firstPeerManager.WaitForDiscoveredPeer();

        var newRumor = new Rumor(new RumorName("State"), new RumorValue("Normal"), RumorVersion.New());
        firstPeerManager.LocalPeer.Apply(new[] { newRumor });

        await Task.Delay(TimeSpan.FromSeconds(5));

        Rumor actualNewRumor = secondHost
            .GetPeerManager()
            .ActiveRemotePeers
            .First(x => x.Address == firstPeerManager.LocalPeer.Address)
            .Rumors
            .FirstOrDefault(x => x.Key == newRumor.Name)
            .Value;

        Assert.Equal(newRumor.Name, actualNewRumor.Name);
        Assert.Equal(newRumor.Value, actualNewRumor.Value);
        Assert.Equal(newRumor.Version, actualNewRumor.Version);
    }
}