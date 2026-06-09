using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ParkingManagement.Web.Realtime;

public sealed class RealtimeUpdateService : IRealtimeUpdateService
{
    private readonly ConcurrentDictionary<Guid, Channel<RealtimeUpdateEvent>> _subscribers = new();

    public RealtimeSubscription Subscribe()
    {
        var id = Guid.NewGuid();
        var channel = Channel.CreateUnbounded<RealtimeUpdateEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _subscribers[id] = channel;

        return new RealtimeSubscription(channel.Reader, () =>
        {
            if (_subscribers.TryRemove(id, out var removed))
            {
                removed.Writer.TryComplete();
            }
        });
    }

    public ValueTask PublishAsync(RealtimeUpdateEvent update)
    {
        foreach (var channel in _subscribers.Values)
        {
            channel.Writer.TryWrite(update);
        }

        return ValueTask.CompletedTask;
    }
}

