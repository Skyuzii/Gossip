using Gossip.Core.Abstractions.Messages.Common;

namespace Gossip.Core.Implementations.Messages.Handlers.Collectors;

internal interface IInfoCollector
{
    (IReadOnlyCollection<DigestPeerInfo>, IReadOnlyCollection<FullPeerInfo>) Collect(IReadOnlyCollection<DigestPeerInfo> digestPeerInfos, bool fillDigestPeerInfos = true);
}