namespace Gossip.Core.Abstractions.Peers;

public readonly record struct PeerAddress(Uri Value)
{
    public bool IsEmpty => Value is null;
}