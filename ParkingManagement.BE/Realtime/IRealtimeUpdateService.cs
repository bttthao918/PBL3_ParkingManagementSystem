using System.Threading.Channels;

namespace ParkingManagement.Web.Realtime;

public interface IRealtimeUpdateService
{
    RealtimeSubscription Subscribe();
    ValueTask PublishAsync(RealtimeUpdateEvent update);
}

public sealed class RealtimeSubscription : IDisposable
{
    private readonly Action _dispose;

    public RealtimeSubscription(ChannelReader<RealtimeUpdateEvent> reader, Action dispose)
    {
        Reader = reader;
        _dispose = dispose;
    }

    public ChannelReader<RealtimeUpdateEvent> Reader { get; }

    public void Dispose() => _dispose();
}

