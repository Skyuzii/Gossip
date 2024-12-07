using Gossip.Core.Abstractions.Peers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gossip.IntegrationsTests.Framework.Extensions;

internal static class HostExtensions
{
    public static IPeerManager GetPeerManager(this IHost host)
    {
        return host.Services.GetRequiredService<IPeerManager>();
    }
}