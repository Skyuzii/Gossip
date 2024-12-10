using Gossip.IntegrationsTests.Framework.Configurations;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gossip.IntegrationsTests.Framework;

internal static class GossiperHostFactory
{
    public static GossiperHost Create(int port, GossiperConfiguration gossiperConfiguration)
    {
        return new GossiperHost(Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webHost => webHost.UseKestrel(options => options.ListenLocalhost(port)).UseStartup<Startup>())
            .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(BuildConfiguration(gossiperConfiguration)))
            .ConfigureLogging(builder => builder.SetMinimumLevel(LogLevel.Debug))
            .Build());
    }

    public static async Task<GossiperHost[]> CreateManyAndStart(int portStart, int count, string[]? remoteStartingPeerAddresses = null)
    {
        var hosts = new List<GossiperHost>();

        foreach (int port in Enumerable.Range(portStart, count))
        {
            GossiperHost host = Create(port: port, new GossiperConfiguration { LocalPeer = $"http://localhost:{port}/", RemoteStartingPeerAddresses = remoteStartingPeerAddresses ?? Array.Empty<string>() });

            await host.Start();

            hosts.Add(host);
        }

        return hosts.ToArray();
    }

    private static Dictionary<string, string?> BuildConfiguration(GossiperConfiguration gossiperConfiguration)
    {
        var configuration = new Dictionary<string, string?>();

        if (!string.IsNullOrWhiteSpace(gossiperConfiguration.LocalPeer))
        {
            configuration["GossiperConfiguration:LocalPeer"] = gossiperConfiguration.LocalPeer;
        }

        for (var i = 0; i < gossiperConfiguration.RemoteStartingPeerAddresses.Length; i++)
        {
            configuration[$"GossiperConfiguration:RemoteStartingPeerAddresses:{i}"] = gossiperConfiguration.RemoteStartingPeerAddresses[i];
        }

        return configuration;
    }
}