using Gossip.Core.Implementations.Messages.Handlers;

namespace Gossip.Core.Abstractions.Peers.Rumors;

public record Rumor(RumorName Name, RumorValue Value, RumorVersion Version)
{
    public static Rumor NextHeartBeat()
    {
        return new Rumor(RumorName.HeartBeat, new RumorValue(HeartBeatGenerator.Next().ToString()), RumorVersion.New());
    }
}