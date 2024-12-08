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
    public async Task StartHost_RemoteStartingPeerShouldDiscoveredNewPeer()
    {
        IHost firstHost = await HostFactory.Create(port: 5169, new GossiperConfiguration { LocalPeer = "http://localhost:5169/" });
        IHost secondHost = await HostFactory.Create(port: 5260, new GossiperConfiguration { LocalPeer = "http://localhost:5260/", RemoteStartingPeerAddresses = new[] { "http://localhost:5169/" } });

        Peer discoveredPeer = await firstHost.GetPeerManager().WaitForDiscoveredPeer();

        Assert.Equal(secondHost.GetPeerManager().LocalPeer.Address, discoveredPeer.Address);
    }

    [Fact]
    public async Task StopHost_RemoteStartingPeerShouldUnreachablePeer()
    {
        IHost firstHost = await HostFactory.Create(port: 5169, new GossiperConfiguration { LocalPeer = "http://localhost:5169/" });
        IHost secondHost = await HostFactory.Create(port: 5260, new GossiperConfiguration { LocalPeer = "http://localhost:5260/", RemoteStartingPeerAddresses = new[] { "http://localhost:5169/" } });

        IPeerManager firstPeerManager = firstHost.GetPeerManager();

        await firstPeerManager.WaitForDiscoveredPeer();

        await secondHost.StopAsync();

        PeerAddress unreachablePeer = await firstPeerManager.WaitForUnreachablePeer();

        Assert.Equal(secondHost.GetPeerManager().LocalPeer.Address, unreachablePeer);
    }

    [Fact]
    public async Task StartHost_SpreadsNewRumor()
    {
        IHost firstHost = await HostFactory.Create(port: 5169, new GossiperConfiguration { LocalPeer = "http://localhost:5169/" });
        IHost secondHost = await HostFactory.Create(port: 5260, new GossiperConfiguration { LocalPeer = "http://localhost:5260/", RemoteStartingPeerAddresses = new[] { "http://localhost:5169/" } });

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

    [Fact]
    public async Task StartManyHost_AllPeersDiscoveredOnEachPeer()
    {
        var remoteStartingPeerHosts = new List<IHost>();

        foreach (int remoteStartingPeerPort in Enumerable.Range(start: 5200, count: 10).Select(x => x))
        {
            remoteStartingPeerHosts.Add(await HostFactory.Create(port: remoteStartingPeerPort, new GossiperConfiguration { LocalPeer = $"http://localhost:{remoteStartingPeerPort}/" }));

            await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(minValue: 100, maxValue: 1000)));
        }

        var peerHosts = new List<IHost>();

        foreach (int peerPort in Enumerable.Range(start: 5300, count: 50).Select(x => x))
        {
            peerHosts.Add(await HostFactory.Create(port: peerPort, new GossiperConfiguration { LocalPeer = $"http://localhost:{peerPort}/", RemoteStartingPeerAddresses = remoteStartingPeerHosts.Select(x => x.GetPeerManager().LocalPeer.Address.Value.ToString()).ToArray() }));

            await Task.Delay(TimeSpan.FromMilliseconds(Random.Shared.Next(minValue: 100, maxValue: 1000)));
        }

        await Task.Delay(TimeSpan.FromSeconds(10));

        int expectedPeerHosts = remoteStartingPeerHosts.Count + peerHosts.Count - 1;
        foreach (IHost peerHost in peerHosts.Concat(remoteStartingPeerHosts))
        {
            IPeerManager peerManager = peerHost.GetPeerManager();

            Assert.Equal(expectedPeerHosts, peerManager.ActiveRemotePeers.Count());
            Assert.Contains(peerHosts.Select(x => x.GetPeerManager()), otherPeerManager => otherPeerManager.LocalPeer.Address != peerManager.LocalPeer.Address && otherPeerManager.ActiveRemotePeers.Any(remotePeer => peerManager.LocalPeer.Address == remotePeer.Address));
        }
    }
}