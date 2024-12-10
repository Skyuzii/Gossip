using System.Diagnostics;

using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;
using Gossip.IntegrationsTests.Framework;
using Gossip.IntegrationsTests.Framework.Configurations;
using Gossip.IntegrationsTests.Framework.Extensions;

using Xunit.Abstractions;

namespace Gossip.IntegrationsTests;

public sealed class GossiperTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public GossiperTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task StartHost_RemoteStartingPeerShouldDiscoveredNewPeer()
    {
        GossiperHost firstHost = GossiperHostFactory.Create(port: 5169, new GossiperConfiguration { LocalPeer = "http://localhost:5169/" });
        GossiperHost secondHost = GossiperHostFactory.Create(port: 5260, new GossiperConfiguration { LocalPeer = "http://localhost:5260/", RemoteStartingPeerAddresses = new[] { "http://localhost:5169/" } });

        Task<Peer> waitForDiscoveredPeerTask = firstHost.PeerManager.WaitForDiscoveredPeer();

        await Task.WhenAll(firstHost.Start(), secondHost.Start(), waitForDiscoveredPeerTask);

        Assert.Equal(secondHost.PeerManager.LocalPeer.Address, waitForDiscoveredPeerTask.Result.Address);
    }

    [Fact]
    public async Task StopHost_RemoteStartingPeerShouldUnreachablePeer()
    {
        GossiperHost firstHost = GossiperHostFactory.Create(port: 5169, new GossiperConfiguration { LocalPeer = "http://localhost:5169/" });
        GossiperHost secondHost = GossiperHostFactory.Create(port: 5260, new GossiperConfiguration { LocalPeer = "http://localhost:5260/", RemoteStartingPeerAddresses = new[] { "http://localhost:5169/" } });

        Task<Peer> waitForDiscoveredPeerTask = firstHost.PeerManager.WaitForDiscoveredPeer();

        await Task.WhenAll(firstHost.Start(), secondHost.Start(), waitForDiscoveredPeerTask);

        await secondHost.Stop();

        PeerAddress unreachablePeer = await firstHost.PeerManager.WaitForUnreachablePeer();

        Assert.Equal(secondHost.PeerManager.LocalPeer.Address, unreachablePeer);
    }

    [Fact]
    public async Task StartHost_SpreadsNewRumor()
    {
        GossiperHost firstHost = GossiperHostFactory.Create(port: 5169, new GossiperConfiguration { LocalPeer = "http://localhost:5169/" });
        GossiperHost secondHost = GossiperHostFactory.Create(port: 5260, new GossiperConfiguration { LocalPeer = "http://localhost:5260/", RemoteStartingPeerAddresses = new[] { "http://localhost:5169/" } });

        Task<Peer> waitForDiscoveredPeerTask = firstHost.PeerManager.WaitForDiscoveredPeer();

        await Task.WhenAll(firstHost.Start(), secondHost.Start(), waitForDiscoveredPeerTask);

        var newRumor = new Rumor(new RumorName("State"), new RumorValue("Normal"), RumorVersion.New());
        firstHost.PeerManager.LocalPeer.Apply(newRumor);

        await Task.Delay(TimeSpan.FromSeconds(5));

        Rumor actualNewRumor = secondHost
            .PeerManager
            .ActiveRemotePeers
            .First(x => x.Address == firstHost.PeerManager.LocalPeer.Address)
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
        GossiperHost[] remoteStartingPeerHosts = await GossiperHostFactory.CreateManyAndStart(
            portStart: 5200,
            count: 10);

        string[] remoteStartingPeerAddresses = remoteStartingPeerHosts.Select(x => x.PeerManager.LocalPeer.Address.Value.ToString()).ToArray();

        GossiperHost[] hosts = await GossiperHostFactory.CreateManyAndStart(
            portStart: 5300,
            count: 50,
            remoteStartingPeerAddresses);

        await Task.Delay(TimeSpan.FromSeconds(10));

        int expectedHostsLength = remoteStartingPeerHosts.Length + hosts.Length - 1;
        foreach (GossiperHost host in remoteStartingPeerHosts.Concat(hosts))
        {
            Assert.Equal(expectedHostsLength, host.PeerManager.ActiveRemotePeers.Count());
            Assert.Contains(hosts, x => x.PeerManager.LocalPeer.Address != host.PeerManager.LocalPeer.Address && x.PeerManager.ActiveRemotePeers.Any(remotePeer => host.PeerManager.LocalPeer.Address == remotePeer.Address));
        }
    }

    // todo: delete
    [Fact]
    public async Task StartManyHost_AllPeersHaveNewRumor_ReturnsTime()
    {
        GossiperHost[] remoteStartingPeerHosts = await GossiperHostFactory.CreateManyAndStart(
            portStart: 5200,
            count: 10);

        string[] remoteStartingPeerAddresses = remoteStartingPeerHosts.Select(x => x.PeerManager.LocalPeer.Address.Value.ToString()).ToArray();

        GossiperHost[] hosts = await GossiperHostFactory.CreateManyAndStart(
            portStart: 5300,
            count: 50,
            remoteStartingPeerAddresses);

        int expectedHostsLength = remoteStartingPeerHosts.Length + hosts.Length - 1;

        foreach (GossiperHost host in remoteStartingPeerHosts.Concat(hosts))
        {
            while (expectedHostsLength != host.PeerManager.ActiveRemotePeers.Count())
            {
                // wait
            }
        }

        GossiperHost rndHost = hosts[Random.Shared.Next(minValue: 0, hosts.Length)];
        var newRumor = new Rumor(new RumorName("State"), new RumorValue("Normal"), RumorVersion.New());

        var sw = Stopwatch.StartNew();
        rndHost.PeerManager.LocalPeer.Apply(newRumor);

        foreach (GossiperHost host in remoteStartingPeerHosts.Concat(hosts).Where(x => x.PeerManager.LocalPeer.Address != rndHost.PeerManager.LocalPeer.Address))
        {
            while (!host.PeerManager.ActiveRemotePeers.Any(x => x.Rumors.Any(rumor => rumor.Key == newRumor.Name)))
            {
                // wait
            }
        }

        sw.Stop();
        _testOutputHelper.WriteLine($"All peers have new rumor for {sw.Elapsed}");
    }

    // todo: delete
    [Fact]
    public async Task StartManyHost_AllPeersDiscoveredOnEachPeer_ReturnsTime()
    {
        GossiperHost[] remoteStartingPeerHosts = await GossiperHostFactory.CreateManyAndStart(
            portStart: 5200,
            count: 10);

        string[] remoteStartingPeerAddresses = remoteStartingPeerHosts.Select(x => x.PeerManager.LocalPeer.Address.Value.ToString()).ToArray();

        GossiperHost[] hosts = await GossiperHostFactory.CreateManyAndStart(
            portStart: 5300,
            count: 50,
            remoteStartingPeerAddresses);

        int expectedHostsLength = remoteStartingPeerHosts.Length + hosts.Length - 1;

        var sw = Stopwatch.StartNew();

        foreach (GossiperHost host in remoteStartingPeerHosts.Concat(hosts))
        {
            while (expectedHostsLength != host.PeerManager.ActiveRemotePeers.Count())
            {
                // wait
            }
        }

        sw.Stop();
        _testOutputHelper.WriteLine($"All peers discovered for {sw.Elapsed}");
    }
}