using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Registrars;

namespace Gossip.Playground;

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
                        .SetMessageSender(serviceProvider => new HttpMessageSender(serviceProvider.GetRequiredService<ILogger<HttpMessageSender>>()))
                        .SetLocalPeerAddress(new PeerAddress(new Uri(gossiperConfig!.LocalPeer)))
                        .SetSyncDigestInMs(10000);

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