using System.Collections.Concurrent;

using Gossip.Core.Abstractions.Peers.Rumors;

namespace Gossip.Core.Abstractions.Peers;

public sealed class Peer
{
    private readonly ConcurrentDictionary<RumorName, Rumor> _rumors;

    public PeerAddress Address { get; }

    public DateTimeOffset RumorsUpdatedAt { get; private set; }

    public bool IsLocal { get; }

    public bool IsRemoteStarting { get; }

    public bool IsRemote { get; }

    public IReadOnlyDictionary<RumorName, Rumor> Rumors => _rumors;

    private Peer(
        PeerAddress address,
        bool isLocal,
        bool isRemote,
        bool isRemoteStarting,
        IReadOnlyCollection<Rumor> rumors)
    {
        Address = address;
        IsLocal = isLocal;
        IsRemote = isRemote;
        IsRemoteStarting = isRemoteStarting;
        _rumors = new ConcurrentDictionary<RumorName, Rumor>(rumors.ToDictionary(x => x.Name, y => y));
    }

    public static Peer CreateLocal(PeerAddress address)
    {
        return new Peer(
            address,
            isLocal: true,
            isRemote: false,
            isRemoteStarting: false,
            new[] { new Rumor(RumorName.HeartBeat, new RumorValue("Good"), RumorVersion.New()) });
    }

    public static Peer CreateRemoteStarting(PeerAddress address)
    {
        return new Peer(
            address,
            isLocal: false,
            isRemote: true,
            isRemoteStarting: true,
            Array.Empty<Rumor>());
    }

    public static Peer CreateRemote(PeerAddress address, IReadOnlyCollection<Rumor> rumors)
    {
        return new Peer(
            address,
            isLocal: false,
            isRemote: true,
            isRemoteStarting: false,
            rumors);
    }

    public RumorVersion GetMaxVersion()
    {
        return _rumors.Values.MaxBy(x => x.Version)?.Version ?? RumorVersion.Empty;
    }

    /// <summary>
    /// Apply rumor
    /// Add if none exists, or update if version is greater
    /// </summary>
    /// <param name="rumor"></param>
    public void Apply(Rumor rumor)
    {
        if (!_rumors.TryGetValue(rumor.Name, out Rumor? existRumor))
        {
            _rumors.TryAdd(rumor.Name, rumor);
            RumorsUpdatedAt = DateTimeOffset.UtcNow;
            return;
        }

        if (rumor.Version > existRumor.Version)
        {
            _rumors[rumor.Name] = rumor;
            RumorsUpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    /// <summary>
    /// Apply rumors
    /// Add if none exists, or update if version is greater
    /// </summary>
    /// <param name="rumors"></param>
    public void Apply(IReadOnlyCollection<Rumor> rumors)
    {
        foreach (Rumor? rumor in rumors)
        {
            Apply(rumor);
        }
    }
}