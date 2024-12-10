namespace Gossip.Core.Implementations.Messages.Handlers;

internal static class HeartBeatGenerator
{
    private static long Value;

    public static long Next()
    {
        return Interlocked.Increment(ref Value);
    }
}