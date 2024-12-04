namespace Gossip.Core.Implementations.Messages.Dispatchers;

public sealed class MessageDispatcherOptions
{
    public int Capacity { get; set; }

    public MessageDispatcherOptions(int capacity)
    {
        Capacity = capacity;
    }
}