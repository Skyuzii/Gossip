using Gossip.Core.Abstractions;
using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Implementations.Gossipers;
using Gossip.Core.Implementations.Managers;
using Gossip.Core.Implementations.Messages.Dispatchers;
using Gossip.Core.Implementations.Messages.Handlers;
using Gossip.Core.Implementations.ReceiversPickers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Gossip.Core.Registrars;

public static class GossiperRegistrars
{
    public static IServiceCollection AddGossiper(
        this IServiceCollection services,
        Action<GossiperConfigBuilder>? configBuilder = null)
    {
        GossiperConfig gossiperConfig = BuildConfig(configBuilder);

        var peerManager = new PeerManager(
            Peer.CreateLocal(gossiperConfig.LocalPeerAddress),
            new PeerManagerOptions(gossiperConfig.ActiveRemotePeersCapacity),
            gossiperConfig.RemoteStartingPeerAddresses.Select(Peer.CreateRemoteStarting).ToArray());

        services.TryAddSingleton<IPeerManager>(peerManager);

        return services.AddSingleton<IGossiper>(
            serviceProvider =>
            {
                IMessageSender messageSender = gossiperConfig.MessageSender.Invoke(serviceProvider);

                IReceiversPicker receiversPicker = gossiperConfig.ReceiversPicker ?? new DefaultReceiversPicker(peerManager);

                var messageHandlers = new IMessageHandler[]
                {
                    new StartRumorDigestMessageHandler(
                        messageSender,
                        peerManager,
                        serviceProvider.GetRequiredService<ILogger<StartRumorDigestMessageHandler>>(),
                        receiversPicker),
                    new RumorDigestMessageHandler(
                        messageSender,
                        peerManager,
                        serviceProvider.GetRequiredService<ILogger<RumorDigestMessageHandler>>()),
                    new RumorDigestAckMessageHandler(
                        messageSender,
                        peerManager,
                        serviceProvider.GetRequiredService<ILogger<RumorDigestAckMessageHandler>>()),
                    new RumorDigestAck2MessageHandler(
                        messageSender,
                        peerManager,
                        serviceProvider.GetRequiredService<ILogger<RumorDigestAck2MessageHandler>>())
                };

                var messageDispatcher = new MessageDispatcher(
                    new MessageDispatcherOptions(gossiperConfig.MessageDispatcherCapacity),
                    messageHandlers);

                return new Gossiper(
                    peerManager,
                    messageDispatcher,
                    new GossiperOptions(gossiperConfig.SyncDigestInMs),
                    serviceProvider.GetRequiredService<ILogger<Gossiper>>());
            });
    }

    private static GossiperConfig BuildConfig(Action<GossiperConfigBuilder>? configBuilder)
    {
        var gossiperConfigBuilderArg = new GossiperConfigBuilder();
        configBuilder?.Invoke(gossiperConfigBuilderArg);

        return gossiperConfigBuilderArg.Build();
    }
}