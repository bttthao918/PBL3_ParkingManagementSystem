using Microsoft.EntityFrameworkCore;
using ParkingManagement.DAL.Data;
using ParkingManagement.DAL.Interfaces;
using ParkingManagement.DAL.Implementations;
using ParkingManagement.DAL.Repositories;
using ParkingManagement.BLL.Interfaces;
using ParkingManagement.BLL.Services;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.BLL.Services.Implementations;
using ParkingManagement.BLL.Validators;
using ParkingManagement.BLL.Strategies;

namespace ParkingManagement.Web.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly("BackendAPI")
                )
            );

            // 2. Repositories (DAL)
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IEmployeeInviteRepository, EmployeeInviteRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IParkingSlotRepository, ParkingSlotRepository>();
            services.AddScoped<IParkingSlotAuditLogRepository, ParkingSlotAuditLogRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<IMonthlyTicketRepository, MonthlyTicketRepository>();
            services.AddScoped<IReservationRepository, ReservationRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IManagerRepository, ManagerRepository>();
            services.AddScoped<IOtpRepository, OtpRepository>();
            services.AddScoped<IPricingRepository, PricingRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // 3. Validators
            services.AddScoped<ICheckInValidator, CheckInValidator>();
            services.AddScoped<IPaymentValidator, PaymentValidator>();
            services.AddScoped<IParkingSlotValidator, ParkingSlotValidator>();

            // 4. Strategies
            services.AddScoped<IParkingSlotStrategy, DefaultParkingSlotStrategy>();

            // 5. Services (BLL)
            services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IParkingService, ParkingService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<ITicketService, TicketService>();
            services.AddScoped<IMonthlyTicketService, MonthlyTicketService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IParkingSlotService, ParkingSlotService>();
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IPricingService, PricingService>();
            services.AddScoped<IReportService, ReportService>();

            return services;
        }
    }
}
