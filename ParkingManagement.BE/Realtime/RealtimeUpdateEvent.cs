namespace ParkingManagement.Web.Realtime;

public sealed record RealtimeUpdateEvent(
    string Topic,
    string Action,
    string? EntityId,
    DateTimeOffset OccurredAt);

