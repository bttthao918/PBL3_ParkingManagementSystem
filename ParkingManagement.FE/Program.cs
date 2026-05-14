using Microsoft.AspNetCore.Authentication.Cookies;
using ParkingManagement.FE.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Razor Pages ──────────────────────────
builder.Services.AddRazorPages();

// ── HttpClient → gọi BE API ──────────────────────────
var backendBaseUrl = builder.Configuration["BackendApi:BaseUrl"]
    ?? throw new InvalidOperationException("BackendApi:BaseUrl not configured");

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
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<ITicketService, TicketService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IReportService, ReportService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<ICustomerApiService, CustomerApiService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IParkingSlotService, ParkingSlotService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IEmployeeService, EmployeeService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IMonthlyTicketService, MonthlyTicketService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IReservationService, ReservationService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IPaymentService, PaymentService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IEmployeeMonthlyTicketService, EmployeeMonthlyTicketService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IParkingOperationService, ParkingOperationService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);
builder.Services.AddHttpClient<IWorkLogService, WorkLogService>(client => ConfigureHttpClient(client))
    .ConfigurePrimaryHttpMessageHandler(CreateHandler);

builder.Services.AddHttpClient<IAccountProfileService, AccountProfileService>(client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

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

app.Run();
