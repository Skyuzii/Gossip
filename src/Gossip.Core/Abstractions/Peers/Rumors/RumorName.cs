namespace Gossip.Core.Abstractions.Peers.Rumors;

public readonly record struct RumorName(string Value)
{
    public static readonly RumorName HeartBeat = new RumorName("HeartBeat");
}