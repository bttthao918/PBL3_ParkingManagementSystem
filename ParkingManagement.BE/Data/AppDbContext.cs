using Microsoft.EntityFrameworkCore;
using ParkingManagement.DAL.Models;

namespace ParkingManagement.DAL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── DbSets ───────────────────────────────────────────────────
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<EmployeeInvite> EmployeeInvites { get; set; }
        public DbSet<Manager> Managers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ParkingSlot> ParkingSlots { get; set; }
        public DbSet<ParkingSlotAuditLog> ParkingSlotAuditLogs { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<MonthlyTicket> MonthlyTickets { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Otp> Otps { get; set; }
        public DbSet<PricingConfiguration> PricingConfigurations { get; set; }
        public DbSet<WorkLog> WorkLogs { get; set; }
        public DbSet<ShiftSchedule> ShiftSchedules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Relationships & Constraints ───────────────────────────
            ConfigureRelationships(modelBuilder);
            ConfigureConstraints(modelBuilder);

            // ── Seed data ─────────────────────────────────────────────
            ParkingManagementSeeder.Seed(modelBuilder);
        }

        private static void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // ── Unique constraints ────────────────────────────────────
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.Email).IsUnique();

            // ── Account → Customer (1-1) ──────────────────────────────
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Customer)
                .WithOne(c => c.Account)
                .HasForeignKey<Customer>(c => c.AccountId);

            // ── Account → Employee (1-1) ──────────────────────────────
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Employee)
                .WithOne(e => e.Account)
                .HasForeignKey<Employee>(e => e.AccountId);

            // ── Account → Manager (1-1) ───────────────────────────────
            modelBuilder.Entity<Account>()
                .HasOne(a => a.Manager)
                .WithOne(m => m.Account)
                .HasForeignKey<Manager>(m => m.AccountId);

            // ── Employee → Payment (1-N, optional) ────────────────────
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.CollectedByEmployee)
                .WithMany()
                .HasForeignKey(p => p.CollectedByEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.MonthlyTicket)
                .WithMany(m => m.Payments)
                .HasForeignKey(p => p.MonthlyTicketId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Manager → Employee (1-N) ──────────────────────────────
            modelBuilder.Entity<Manager>()
                .HasMany(m => m.Employees)
                .WithOne(e => e.Manager)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── OTP Configuration ─────────────────────────────────────
            modelBuilder.Entity<Otp>()
                .HasIndex(o => o.Email);

            modelBuilder.Entity<WorkLog>()
                .HasOne(w => w.ShiftSchedule)
                .WithMany(s => s.WorkLogs)
                .HasForeignKey(w => w.ScheduleId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private static void ConfigureConstraints(ModelBuilder modelBuilder)
        {
            // ── Payment: chỉ 1 trong 2 FK có giá trị ─────────────────
            modelBuilder.Entity<Payment>()
                .ToTable(tb => tb.HasCheckConstraint(
                    "CK_Payment_TicketOrMonthly",
                    "(TicketId IS NOT NULL AND MonthlyTicketId IS NULL) OR (TicketId IS NULL AND MonthlyTicketId IS NOT NULL)"));

            // ── Check constraints ─────────────────────────────────────
            modelBuilder.Entity<ParkingSlot>()
                .ToTable(tb => tb.HasCheckConstraint(
                    "CK_ParkingSlot_Status",
                    "Status IN (N'Trống', N'Đang sử dụng', N'Đã đặt', N'Bảo trì')"));

            modelBuilder.Entity<Vehicle>()
                .ToTable(tb => tb.HasCheckConstraint(
                    "CK_Vehicle_Type",
                    "VehicleType IN (N'Xe máy', N'Ô tô nhỏ', N'Ô tô lớn')"));

            modelBuilder.Entity<Ticket>()
                .ToTable(tb => tb.HasCheckConstraint(
                    "CK_Ticket_Status",
                    "Status IN (N'Đang trong bãi', N'Đã ra')"));

            modelBuilder.Entity<MonthlyTicket>()
                .ToTable(tb => tb.HasCheckConstraint(
                    "CK_MonthlyTicket_Status",
                    "Status IN (N'Hoạt động', N'Hết hạn', N'Đã hủy', N'Chờ thanh toán')"));

            modelBuilder.Entity<Reservation>()
                .ToTable(tb => tb.HasCheckConstraint(
                    "CK_Reservation_Status",
                    "Status IN (N'Chờ', N'Đã nhận', N'Hủy', N'Hết hạn')"));
        }
    }
}
