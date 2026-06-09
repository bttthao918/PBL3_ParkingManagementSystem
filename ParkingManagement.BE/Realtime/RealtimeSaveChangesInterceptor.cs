using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ParkingManagement.DAL.Models;

namespace ParkingManagement.Web.Realtime;

public sealed class RealtimeSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IRealtimeUpdateService _updates;
    private readonly ConcurrentDictionary<Guid, List<RealtimeUpdateEvent>> _pendingUpdates = new();

    public RealtimeSaveChangesInterceptor(IRealtimeUpdateService updates)
    {
        _updates = updates;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureUpdates(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureUpdates(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PublishCapturedUpdates(eventData.Context);
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await PublishCapturedUpdatesAsync(eventData.Context);
        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        ClearCapturedUpdates(eventData.Context);
    }

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        ClearCapturedUpdates(eventData.Context);
        return Task.CompletedTask;
    }

    private void CaptureUpdates(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var updates = context.ChangeTracker
            .Entries()
            .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .SelectMany(CreateUpdates)
            .GroupBy(update => new { update.Topic, update.Action, update.EntityId })
            .Select(group => group.First())
            .ToList();

        if (updates.Count == 0)
        {
            _pendingUpdates.TryRemove(context.ContextId.InstanceId, out _);
            return;
        }

        _pendingUpdates[context.ContextId.InstanceId] = updates;
    }

    private IEnumerable<RealtimeUpdateEvent> CreateUpdates(EntityEntry entry)
    {
        var action = entry.State switch
        {
            EntityState.Added => "created",
            EntityState.Modified => "updated",
            EntityState.Deleted => "deleted",
            _ => "changed"
        };

        var entityId = GetPrimaryKeyValue(entry);
        var now = DateTimeOffset.UtcNow;

        foreach (var topic in GetTopics(entry.Entity))
        {
            yield return new RealtimeUpdateEvent(topic, action, entityId, now);
        }
    }

    private static IEnumerable<string> GetTopics(object entity)
    {
        switch (entity)
        {
            case Reservation:
                yield return "reservations";
                yield return "parking-slots";
                yield return "dashboard";
                break;
            case ParkingSlot:
            case ParkingSlotAuditLog:
                yield return "parking-slots";
                yield return "dashboard";
                break;
            case Ticket:
                yield return "tickets";
                yield return "parking-slots";
                yield return "dashboard";
                break;
            case MonthlyTicket:
                yield return "monthly-tickets";
                yield return "dashboard";
                break;
            case Payment:
                yield return "payments";
                yield return "tickets";
                yield return "monthly-tickets";
                yield return "dashboard";
                break;
            case PricingConfiguration:
                yield return "pricing";
                yield return "tickets";
                yield return "monthly-tickets";
                break;
            case Customer:
            case Vehicle:
            case Account:
                yield return "customers";
                yield return "dashboard";
                break;
            case Employee:
            case ShiftSchedule:
            case WorkLog:
                yield return "employees";
                yield return "dashboard";
                break;
        }
    }

    private static string? GetPrimaryKeyValue(EntityEntry entry)
    {
        var primaryKey = entry.Metadata.FindPrimaryKey();
        if (primaryKey is null)
        {
            return null;
        }

        var values = primaryKey.Properties
            .Select(property => entry.Property(property.Name).CurrentValue?.ToString())
            .Where(value => !string.IsNullOrWhiteSpace(value));

        return string.Join(":", values);
    }

    private void PublishCapturedUpdates(DbContext? context)
    {
        if (context is null || !_pendingUpdates.TryRemove(context.ContextId.InstanceId, out var updates))
        {
            return;
        }

        foreach (var update in updates)
        {
            _updates.PublishAsync(update).GetAwaiter().GetResult();
        }
    }

    private async ValueTask PublishCapturedUpdatesAsync(DbContext? context)
    {
        if (context is null || !_pendingUpdates.TryRemove(context.ContextId.InstanceId, out var updates))
        {
            return;
        }

        foreach (var update in updates)
        {
            await _updates.PublishAsync(update);
        }
    }

    private void ClearCapturedUpdates(DbContext? context)
    {
        if (context is not null)
        {
            _pendingUpdates.TryRemove(context.ContextId.InstanceId, out _);
        }
    }
}

