using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Registrars;
using Gossip.IntegrationsTests.Framework.Configurations;
using Gossip.IntegrationsTests.Framework.Constants;
using Gossip.IntegrationsTests.Framework.HostedServices;
using Gossip.IntegrationsTests.Framework.MessageSenders;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gossip.IntegrationsTests.Framework;

public sealed class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        var gossiperConfig = Configuration.GetSection(nameof(GossiperConfiguration)).Get<GossiperConfiguration>();

        services
            .AddGossiper(
                builder =>
                {
                    builder
                        .SetLocalPeerAddress(new PeerAddress(new Uri(gossiperConfig!.LocalPeer)))
                        .SetSyncDigestInMs(gossiperConfig.SyncDigestInMs)
                        .SetActiveRemotePeersCapacity(gossiperConfig.ActiveRemotePeersCapacity)
                        .SetUnreachableRemotePeersCapacity(gossiperConfig.UnreachableRemotePeersCapacity)
                        .SetMessageDispatcherCapacity(gossiperConfig.MessageDispatcherCapacity)
                        .SetMessageSender(
                            serviceProvider => new HttpMessageSender(
                                serviceProvider.GetRequiredService<ILogger<HttpMessageSender>>(),
                                serviceProvider.GetRequiredService<IPeerManager>()));

                    if (gossiperConfig.RemoteStartingPeerAddresses.Length > 0)
                    {
                        builder.SetRemoteStartingPeerAddresses(gossiperConfig.RemoteStartingPeerAddresses.Select(peerAddress => new PeerAddress(new Uri(peerAddress))).ToArray());
                    }
                })
            .AddHostedService<HttpHostedService>()
            .AddSingleton<HttpHostedService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.Map(
                HttpConstants.GossiperPath,
                builder =>
                {
                    builder.Run(builder.ApplicationServices.GetRequiredService<HttpHostedService>().ProcessRequest);
                })
            .Map("/peers",
                builder =>
                {
                    builder.Run(builder.ApplicationServices.GetRequiredService<HttpHostedService>().ReturnPeers);
                })
            .Map("/add",
                builder =>
                {
                    builder.Run(builder.ApplicationServices.GetRequiredService<HttpHostedService>().AddRumor);
                })
            .UseRouting();
    }
}