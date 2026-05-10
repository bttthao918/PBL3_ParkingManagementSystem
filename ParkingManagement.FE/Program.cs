using Microsoft.AspNetCore.Authentication.Cookies;
using ParkingManagement.FE.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Razor Pages ──────────────────────────
builder.Services.AddRazorPages();

// ── HttpContextAccessor ──────────────────────────
builder.Services.AddHttpContextAccessor();

// ── Config ──────────────────────────
var backendBaseUrl = builder.Configuration["BackendApi:BaseUrl"]
    ?? throw new InvalidOperationException("BackendApi:BaseUrl not configured");

// ── Auth Service ──────────────────────────
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Customer Service ──────────────────────────
builder.Services.AddHttpClient<ICustomerService, CustomerService>(client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Parking Slot Service ──────────────────────────
builder.Services.AddHttpClient<IParkingSlotService, ParkingSlotService>(client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Reservation Service ──────────────────────────
builder.Services.AddHttpClient<IReservationService, ReservationService>(client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Ticket Service ──────────────────────────
builder.Services.AddHttpClient<ITicketService, TicketService>(client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Monthly Ticket Service ──────────────────────────
builder.Services.AddHttpClient<IMonthlyTicketService, MonthlyTicketService>(client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Payment Service ──────────────────────────
builder.Services.AddHttpClient<IPaymentService, PaymentService>(client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Employee Service ──────────────────────────
builder.Services.AddHttpClient<IEmployeeService, EmployeeService>(client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Report Service ──────────────────────────
builder.Services.AddHttpClient<IReportService, ReportService>(client =>
{
    client.BaseAddress = new Uri(backendBaseUrl);
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

// ── Session ──────────────────────────
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromHours(24);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.Cookie.Name = "ParkingSession";
});

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