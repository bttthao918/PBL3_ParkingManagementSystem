using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services;
using ParkingManagement.BLL.Services.Implementations;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Data;
using ParkingManagement.Web.Extensions;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ── JWT Configuration ────────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT:SecretKey not configured");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "ParkingManagement",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "ParkingManagementUser",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();

// ── Email Service ────────────────────────────
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IEmailService, EmailService>();

// ── Application Services ────────────────────────────
builder.Services.AddApplicationServices(builder.Configuration);

// ── CORS ────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

// ── Controllers + Session ────────────────────────────
builder.Services.AddControllersWithViews();
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(30);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Parking Management API",
        Version = "v1",
        Description = "API"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Hãy nhập: Bearer [Token_của_bạn]",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    c.CustomOperationIds(api => api.ActionDescriptor.RouteValues["controller"] + "_" + api.HttpMethod + "_" + api.RelativePath);
});

var app = builder.Build();

// ✅ Auto migrate + seed — dùng AppDbContext
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        db.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[dbo].[Accounts]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[Customers]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[Employees]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[ParkingSlots]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[PricingConfigurations]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[Tickets]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[Payments]', N'U') IS NOT NULL
            BEGIN
                IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [dbo].[__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL,
                        [ProductVersion] nvarchar(32) NOT NULL,
                        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                    );
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM [dbo].[__EFMigrationsHistory]
                    WHERE [MigrationId] = N'20260514054735_InitialCreate'
                )
                BEGIN
                    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES (N'20260514054735_InitialCreate', N'10.0.6');
                END;
            END;
            """);

        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration was skipped. Check the SQL Server connection before using data endpoints. {ex.Message}");
    }

    try
    {
        db.Database.ExecuteSqlRaw("""
            SET QUOTED_IDENTIFIER ON;

            IF OBJECT_ID(N'[dbo].[Payments]', N'U') IS NOT NULL
            BEGIN
                UPDATE [dbo].[Payments]
                SET [TicketId] = NULL
                WHERE [TicketId] IS NOT NULL
                  AND LTRIM(RTRIM([TicketId])) = N''
                  AND [MonthlyTicketId] IS NOT NULL
                  AND LTRIM(RTRIM([MonthlyTicketId])) <> N'';

                UPDATE [dbo].[Payments]
                SET [MonthlyTicketId] = NULL
                WHERE [MonthlyTicketId] IS NOT NULL
                  AND LTRIM(RTRIM([MonthlyTicketId])) = N''
                  AND [TicketId] IS NOT NULL
                  AND LTRIM(RTRIM([TicketId])) <> N'';

                IF COL_LENGTH(N'[dbo].[Payments]', N'CollectedByEmployeeId') IS NULL
                    ALTER TABLE [dbo].[Payments] ADD [CollectedByEmployeeId] nvarchar(20) NULL;

                IF COL_LENGTH(N'[dbo].[Payments]', N'CollectedByEmployeeId') IS NOT NULL
                   AND NOT EXISTS (
                       SELECT 1
                       FROM sys.indexes
                       WHERE [name] = N'IX_Payments_CollectedByEmployeeId'
                         AND [object_id] = OBJECT_ID(N'[dbo].[Payments]')
                   )
                    CREATE INDEX [IX_Payments_CollectedByEmployeeId]
                        ON [dbo].[Payments] ([CollectedByEmployeeId]);

                IF OBJECT_ID(N'[dbo].[Employees]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'[dbo].[Payments]', N'CollectedByEmployeeId') IS NOT NULL
                   AND NOT EXISTS (
                       SELECT 1
                       FROM sys.foreign_keys
                       WHERE [name] = N'FK_Payments_Employees_CollectedByEmployeeId'
                   )
                    ALTER TABLE [dbo].[Payments] WITH CHECK ADD CONSTRAINT [FK_Payments_Employees_CollectedByEmployeeId]
                        FOREIGN KEY ([CollectedByEmployeeId]) REFERENCES [dbo].[Employees] ([EmployeeId]);

                IF OBJECT_ID(N'[dbo].[__EFMigrationsHistory]', N'U') IS NOT NULL
                   AND COL_LENGTH(N'[dbo].[Payments]', N'CollectedByEmployeeId') IS NOT NULL
                   AND NOT EXISTS (
                       SELECT 1
                       FROM [dbo].[__EFMigrationsHistory]
                       WHERE [MigrationId] = N'20260515180000_AddPaymentEmployeeAttribution'
                   )
                    INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES (N'20260515180000_AddPaymentEmployeeAttribution', N'10.0.6');
            END
            """);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Payments employee attribution schema repair was skipped. {ex.Message}");
    }

    try
    {
        db.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[dbo].[MonthlyTickets]', N'U') IS NOT NULL
            BEGIN
                IF EXISTS (
                    SELECT 1
                    FROM sys.check_constraints
                    WHERE [name] = N'CK_MonthlyTicket_Status'
                      AND parent_object_id = OBJECT_ID(N'[dbo].[MonthlyTickets]')
                )
                    ALTER TABLE [dbo].[MonthlyTickets] DROP CONSTRAINT [CK_MonthlyTicket_Status];

                ALTER TABLE [dbo].[MonthlyTickets] WITH CHECK ADD CONSTRAINT [CK_MonthlyTicket_Status]
                    CHECK ([Status] IN (N'Hoạt động', N'Hết hạn', N'Đã hủy', N'Chờ thanh toán'));
            END
            """);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"MonthlyTickets status schema repair was skipped. {ex.Message}");
    }

    try
    {
        db.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[dbo].[WorkLogs]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[WorkLogs] (
                    [WorkLogId] nvarchar(20) NOT NULL,
                    [EmployeeId] nvarchar(20) NOT NULL,
                    [WorkDate] datetime2 NOT NULL,
                    [StartTime] datetime2 NOT NULL,
                    [EndTime] datetime2 NULL,
                    [TotalMinutes] int NULL,
                    [ScheduleId] nvarchar(20) NULL,
                    [Note] nvarchar(200) NULL,
                    [Status] nvarchar(20) NOT NULL,
                    CONSTRAINT [PK_WorkLogs] PRIMARY KEY ([WorkLogId]),
                    CONSTRAINT [FK_WorkLogs_Employees_EmployeeId]
                        FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([EmployeeId]) ON DELETE CASCADE
                );

                CREATE INDEX [IX_WorkLogs_EmployeeId] ON [dbo].[WorkLogs] ([EmployeeId]);
            END

            IF OBJECT_ID(N'[dbo].[WorkLogs]', N'U') IS NOT NULL
               AND COL_LENGTH(N'[dbo].[WorkLogs]', N'ScheduleId') IS NULL
            BEGIN
                ALTER TABLE [dbo].[WorkLogs] ADD [ScheduleId] nvarchar(20) NULL;
            END

            IF OBJECT_ID(N'[dbo].[WorkLogs]', N'U') IS NOT NULL
               AND COL_LENGTH(N'[dbo].[WorkLogs]', N'ScheduleId') IS NOT NULL
               AND NOT EXISTS (
                    SELECT 1 FROM sys.indexes
                    WHERE name = N'IX_WorkLogs_ScheduleId'
                      AND object_id = OBJECT_ID(N'[dbo].[WorkLogs]')
               )
            BEGIN
                CREATE INDEX [IX_WorkLogs_ScheduleId] ON [dbo].[WorkLogs] ([ScheduleId]);
            END

            IF OBJECT_ID(N'[dbo].[ShiftSchedules]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[WorkLogs]', N'U') IS NOT NULL
               AND EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = N'FK_WorkLogs_ShiftSchedules_ScheduleId'
                      AND delete_referential_action_desc <> N'NO_ACTION'
               )
            BEGIN
                ALTER TABLE [dbo].[WorkLogs] DROP CONSTRAINT [FK_WorkLogs_ShiftSchedules_ScheduleId];
            END

            IF OBJECT_ID(N'[dbo].[ShiftSchedules]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[WorkLogs]', N'U') IS NOT NULL
               AND NOT EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = N'FK_WorkLogs_ShiftSchedules_ScheduleId'
               )
            BEGIN
                ALTER TABLE [dbo].[WorkLogs]
                ADD CONSTRAINT [FK_WorkLogs_ShiftSchedules_ScheduleId]
                    FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[ShiftSchedules] ([ScheduleId]) ON DELETE NO ACTION;
            END
            """);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"WorkLogs schema repair was skipped. {ex.Message}");
    }

    try
    {
        db.Database.ExecuteSqlRaw("""
            IF OBJECT_ID(N'[dbo].[ShiftSchedules]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[ShiftSchedules] (
                    [ScheduleId] nvarchar(20) NOT NULL,
                    [EmployeeId] nvarchar(20) NOT NULL,
                    [WorkDate] datetime2 NOT NULL,
                    [ShiftType] nvarchar(20) NOT NULL,
                    [StartTime] time NOT NULL,
                    [EndTime] time NOT NULL,
                    [Status] nvarchar(20) NOT NULL,
                    [Note] nvarchar(200) NULL,
                    [CreatedBy] nvarchar(20) NOT NULL,
                    [CreatedAt] datetime2 NOT NULL,
                    CONSTRAINT [PK_ShiftSchedules] PRIMARY KEY ([ScheduleId]),
                    CONSTRAINT [FK_ShiftSchedules_Employees_EmployeeId]
                        FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employees] ([EmployeeId]) ON DELETE CASCADE
                );

                CREATE INDEX [IX_ShiftSchedules_EmployeeId] ON [dbo].[ShiftSchedules] ([EmployeeId]);
            END

            IF OBJECT_ID(N'[dbo].[ShiftSchedules]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[WorkLogs]', N'U') IS NOT NULL
               AND COL_LENGTH(N'[dbo].[WorkLogs]', N'ScheduleId') IS NOT NULL
               AND EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = N'FK_WorkLogs_ShiftSchedules_ScheduleId'
                      AND delete_referential_action_desc <> N'NO_ACTION'
               )
            BEGIN
                ALTER TABLE [dbo].[WorkLogs] DROP CONSTRAINT [FK_WorkLogs_ShiftSchedules_ScheduleId];
            END

            IF OBJECT_ID(N'[dbo].[ShiftSchedules]', N'U') IS NOT NULL
               AND OBJECT_ID(N'[dbo].[WorkLogs]', N'U') IS NOT NULL
               AND COL_LENGTH(N'[dbo].[WorkLogs]', N'ScheduleId') IS NOT NULL
               AND NOT EXISTS (
                    SELECT 1
                    FROM sys.foreign_keys
                    WHERE name = N'FK_WorkLogs_ShiftSchedules_ScheduleId'
               )
            BEGIN
                ALTER TABLE [dbo].[WorkLogs]
                ADD CONSTRAINT [FK_WorkLogs_ShiftSchedules_ScheduleId]
                    FOREIGN KEY ([ScheduleId]) REFERENCES [dbo].[ShiftSchedules] ([ScheduleId]) ON DELETE NO ACTION;
            END
            """);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ShiftSchedules schema repair was skipped. {ex.Message}");
    }

    try
    {
        DemoCustomerDataSeeder.Seed(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Demo customer data seed was skipped. {ex.Message}");
    }
}

// ... phần còn lại giữ nguyên

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Only redirect to HTTPS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("Frontend");
app.UseSession();

// ── Authentication & Authorization Middleware ────────────────────────────
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapControllerRoute(
    name: "legacy",
    pattern: "{controller=Auth}/{action=Login}/{id?}");
app.MapFallbackToFile("index.html");

app.Run();
