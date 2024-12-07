using Gossip.Core.Abstractions.Peers;

namespace Gossip.IntegrationsTests.Framework.Extensions;

internal static class PeerManagerExtensions
{
    public static async Task<Peer> WaitForDiscoveredPeer(
        this IPeerManager peerManager,
        PeerAddress? address = null,
        int timeout = 30)
    {
        var tcs = new TaskCompletionSource<Peer>();

        Action<Peer> onDiscoveredNewPeer = (peer) =>
        {
            if (address is null || address == peer.Address)
            {
                tcs.SetResult(peer);
            }
        };

        peerManager.DiscoveredNewPeer += onDiscoveredNewPeer;
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(timeout));
        peerManager.DiscoveredNewPeer -= onDiscoveredNewPeer;

        return tcs.Task.Result;
    }

    public static async Task<PeerAddress> WaitForUnreachablePeer(
        this IPeerManager peerManager,
        PeerAddress? address = null,
        int timeout = 30)
    {
        var tcs = new TaskCompletionSource<PeerAddress>();

        Action<PeerAddress> onNewUnreachablePeer = (unreachablePeerAddress) =>
        {
            if (address is null || address == unreachablePeerAddress)
            {
                tcs.SetResult(unreachablePeerAddress);
            }
        };

        peerManager.NewUnreachablePeer += onNewUnreachablePeer;
        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(timeout));
        peerManager.NewUnreachablePeer -= onNewUnreachablePeer;

        return tcs.Task.Result;
    }
}