using Gossip.IntegrationsTests.Framework.Configurations;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Gossip.IntegrationsTests.Framework;

internal static class HostFactory
{
    public static async Task<IHost> Create(int port, GossiperConfiguration gossiperConfiguration)
    {
        IHost host = Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webHost => webHost.UseKestrel(options => options.ListenLocalhost(port)).UseStartup<Startup>())
            .ConfigureAppConfiguration(builder => builder.AddInMemoryCollection(BuildConfiguration(gossiperConfiguration)))
            .ConfigureLogging(static builder => builder.SetMinimumLevel(LogLevel.Debug))
            .Build();

        await host.StartAsync();

        return host;
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