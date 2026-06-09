using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using ParkingManagement.FE.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var dataProtectionKeysPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtectionKeys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));

// ── Razor Pages ──────────────────────────
builder.Services.AddRazorPages();

// ── HttpClient → gọi BE API ──────────────────────────
var backendBaseUrl = builder.Configuration["BackendApi:BaseUrl"]
    ?? throw new InvalidOperationException("BackendApi:BaseUrl not configured");

// Đăng ký Handler đính kèm JWT Token
builder.Services.AddTransient<AuthDelegatingHandler>();

void ConfigureHttpClient(HttpClient client)
{
    client.BaseAddress = new Uri(backendBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
}

// Handler that trusts dev certs and doesn't lose auth headers on redirect
HttpClientHandler CreateHandler() => new HttpClientHandler
{
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};

builder.Services.AddHttpClient<IAuthService, AuthService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<ITicketService, TicketService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IPricingService, PricingService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IReportService, ReportService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<ICustomerApiService, CustomerApiService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IParkingSlotService, ParkingSlotService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IEmployeeService, EmployeeService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IMonthlyTicketService, MonthlyTicketService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IReservationService, ReservationService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IPaymentService, PaymentService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IEmployeeMonthlyTicketService, EmployeeMonthlyTicketService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IParkingOperationService, ParkingOperationService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IWorkLogService, WorkLogService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IShiftScheduleService, ShiftScheduleService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);

builder.Services.AddHttpClient<IAccountProfileService, AccountProfileService>(client => ConfigureHttpClient(client))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);

builder.Services.AddHttpClient("BackendRealtime", client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
    client.Timeout = Timeout.InfiniteTimeSpan;
})
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);

builder.Services.AddScoped<INotificationService, NotificationService>();

// ── Cookie Authentication ──────────────────────────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Authenticate";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Auth/Authenticate";
        options.Cookie.Name = "ParkingAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ── Session (dùng lưu token để gọi BE các trang khác) ──────────────────────────
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromHours(24);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.Cookie.Name = "ParkingSession";
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapGet("/realtime/stream", async (
    HttpContext context,
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("RealtimeProxy");
    context.Response.Headers.CacheControl = "no-cache";
    context.Response.Headers.Append("X-Accel-Buffering", "no");
    context.Response.ContentType = "text/event-stream";

    try
    {
        var client = httpClientFactory.CreateClient("BackendRealtime");
        using var backendResponse = await client.GetAsync(
            "api/realtime/stream",
            HttpCompletionOption.ResponseHeadersRead,
            context.RequestAborted);

        if (!backendResponse.IsSuccessStatusCode)
        {
            await context.Response.WriteAsync(
                $"event: parking-error\ndata: {{\"status\":{(int)backendResponse.StatusCode}}}\n\n",
                context.RequestAborted);
            return;
        }

        await using var backendStream = await backendResponse.Content.ReadAsStreamAsync(context.RequestAborted);
        await backendStream.CopyToAsync(context.Response.Body, context.RequestAborted);
    }
    catch (OperationCanceledException)
    {
        // The browser closed the EventSource connection.
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Realtime proxy disconnected");
        await context.Response.WriteAsync("event: parking-error\ndata: {}\n\n");
    }
}).RequireAuthorization();

app.Run();
