using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagement.Web.Realtime;

namespace ParkingManagement.Web.Controllers.Api;

[ApiController]
[AllowAnonymous]
[Route("api/realtime")]
public sealed class RealtimeController : ControllerBase
{
    private readonly IRealtimeUpdateService _updates;

    public RealtimeController(IRealtimeUpdateService updates)
    {
        _updates = updates;
    }

    [HttpGet("stream")]
    public async Task Stream(CancellationToken cancellationToken)
    {
        Response.Headers.CacheControl = "no-cache";
        Response.Headers.Append("X-Accel-Buffering", "no");
        Response.ContentType = "text/event-stream";

        using var subscription = _updates.Subscribe();
        try
        {
            await Response.WriteAsync(": connected\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                var readTask = subscription.Reader.WaitToReadAsync(cancellationToken).AsTask();
                var keepAliveTask = Task.Delay(TimeSpan.FromSeconds(20), cancellationToken);
                var completedTask = await Task.WhenAny(readTask, keepAliveTask);

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (completedTask == keepAliveTask)
                {
                    await Response.WriteAsync(": keep-alive\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                    continue;
                }

                if (!await readTask)
                {
                    break;
                }

                while (subscription.Reader.TryRead(out var update))
                {
                    var json = JsonSerializer.Serialize(update);
                    await Response.WriteAsync("event: parking-update\n", cancellationToken);
                    await Response.WriteAsync($"data: {json}\n\n", cancellationToken);
                }

                await Response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected when the browser reloads, navigates away, or closes the tab.
        }
        catch (IOException) when (cancellationToken.IsCancellationRequested)
        {
            // The client disconnected while the server was writing an SSE event.
        }
    }
}
