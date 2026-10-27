# Gossip

A .NET 8 library for peer discovery and decentralized state sharing through a gossip protocol.

Gossip helps nodes in a distributed application learn about one another and exchange metadata without a central coordinator. A node starts with a few known peers, then gradually discovers others and receives their advertised state.

## What it does

- Discovers peers through periodic gossip exchanges.
- Spreads versioned key-value data, called **rumors**, such as a node’s role or application status.
- Tracks active and unreachable peers based on message delivery results.
- Supports custom transports and peer-selection strategies through dependency injection.

## How it works

Nodes periodically exchange summaries of the peers and rumor versions they know. They compare those summaries, request missing information, and share newer state. Updates then spread through subsequent exchanges.

For example, a worker can publish `State = Normal`, and other nodes can learn that value through their peers without the worker contacting every node directly.

State is kept in memory and propagates asynchronously, so nodes may temporarily have different views. Gossip is suited to peer discovery and metadata sharing where immediate consistency is not required.

## Quick start

### Try it locally

Install the .NET 8 SDK and run the rumor-propagation test from the repository root:

> The current `Gossip.Core.csproj` references a logging DLL from a local ASP.NET Core 8.0.5 installation. Replace that machine-specific reference with a `Microsoft.Extensions.Logging.Abstractions` package reference, or update its path, before building.

```sh
dotnet restore Gossip.sln
dotnet test tests/Gossip.IntegrationsTests/Gossip.IntegrationsTests.csproj --filter "FullyQualifiedName~StartHost_SpreadsNewRumor"
```

This test starts two HTTP nodes on localhost ports `5169` and `5260`, publishes a rumor, and checks that it reaches the other node.

### Use it in your application

Add a project reference to `src/Gossip.Core/Gossip.Core.csproj`. Provide an `IMessageSender` implementation for your transport; the test harness's [HTTP sender](tests/Gossip.IntegrationsTests/Framework/MessageSenders/HttpMessageSender.cs) is a reference example.

Register the node in your application's service collection. Here, `MyMessageSender` represents your own implementation:

```csharp
using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Registrars;
using Microsoft.Extensions.DependencyInjection;

services.AddLogging();
services.AddSingleton<IMessageSender, MyMessageSender>();
services.AddGossiper(config => config
    .SetLocalPeerAddress(new PeerAddress(new Uri("http://localhost:5260/")))
    .SetRemoteStartingPeerAddresses(new[]
    {
        new PeerAddress(new Uri("http://localhost:5169/"))
    })
    .SetMessageSender(sp => sp.GetRequiredService<IMessageSender>()));
```

Your host must receive and deserialize incoming messages, then pass them to `IGossiper.Enqueue`. Call `IGossiper.Start` on startup and `Stop` on shutdown. `Start` returns the running loop's task: retain and observe it without awaiting it during startup. See [HttpHostedService](tests/Gossip.IntegrationsTests/Framework/HostedServices/HttpHostedService.cs) for an example.

Once the host is running, publish state through its service provider:

```csharp
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;
using Microsoft.Extensions.DependencyInjection;

var peers = serviceProvider.GetRequiredService<IPeerManager>();
peers.LocalPeer.Apply(new Rumor(
    new RumorName("State"),
    new RumorValue("Normal"),
    RumorVersion.New()));
```

Connected peers receive the update through subsequent gossip exchanges. Read their advertised state from `peers.ActiveRemotePeers`.
