using Microsoft.EntityFrameworkCore;
using ParkingManagement.DAL.Data;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;
using ParkingManagement.DAL.Repositories;

namespace ParkingManagement.DAL.Implementations
{
    // ── CustomerRepository ───────────────────────────────────
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _db;
        public CustomerRepository(AppDbContext db) => _db = db;

        public Task<List<Customer>> GetAllAsync(bool includeDeleted = false) =>
            _db.Customers
               .Include(c => c.Account)
               .Where(c => includeDeleted || !c.IsDeleted)
               .ToListAsync();

        public Task<Customer?> GetByIdAsync(string id) =>
            _db.Customers.Include(c => c.Account)
                         .Include(c => c.Vehicles)
                         .FirstOrDefaultAsync(c => c.CustomerId == id && !c.IsDeleted);

        public Task<Customer?> GetByAccountIdAsync(string accountId) =>
            _db.Customers.Include(c => c.Account)
                         .FirstOrDefaultAsync(c => c.AccountId == accountId && !c.IsDeleted);

        public Task<List<Customer>> SearchAsync(string keyword) =>
            _db.Customers
               .Include(c => c.Account)
               .Include(c => c.Vehicles)
               .Where(c => !c.IsDeleted && (
                   c.FullName.Contains(keyword) ||
                   c.PhoneNumber!.Contains(keyword) ||
                   c.Account.Email!.Contains(keyword) ||
                   c.Vehicles.Any(v => v.VehiclePlate.Contains(keyword))
               ))
               .ToListAsync();

        public Task<List<Customer>> SearchAdvancedAsync(string? fullName, string? phoneNumber, string? email, string? vehiclePlate, int maxResults = 50) =>
            _db.Customers
               .Include(c => c.Account)
               .Include(c => c.Vehicles)
               .Where(c => !c.IsDeleted && (
                   (string.IsNullOrEmpty(fullName) || c.FullName.Contains(fullName)) &&
                   (string.IsNullOrEmpty(phoneNumber) || c.PhoneNumber!.Contains(phoneNumber)) &&
                   (string.IsNullOrEmpty(email) || c.Account.Email!.Contains(email)) &&
                   (string.IsNullOrEmpty(vehiclePlate) || c.Vehicles.Any(v => v.VehiclePlate.Contains(vehiclePlate)))
               ))
               .Take(maxResults)
               .ToListAsync();

        public Task<List<Customer>> GetDeletedAsync() =>
            _db.Customers.Where(c => c.IsDeleted).ToListAsync();

        public async Task AddAsync(Customer customer)
        {
            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer customer)
        {
            _db.Customers.Update(customer);
            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(string id)
        {
            var c = await _db.Customers.FindAsync(id);
            if (c != null) { c.IsDeleted = true; await _db.SaveChangesAsync(); }
        }

        public async Task RestoreAsync(string id)
        {
            var c = await _db.Customers.FindAsync(id);
            if (c != null) { c.IsDeleted = false; await _db.SaveChangesAsync(); }
        }
    }

    // ── EmployeeRepository ───────────────────────────────────
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _db;
        public EmployeeRepository(AppDbContext db) => _db = db;

        public Task<List<Employee>> GetAllAsync(bool includeDeleted = false) =>
            _db.Employees
               .Include(e => e.Account)
               .Where(e => includeDeleted || !e.IsDeleted)
               .ToListAsync();

        public Task<Employee?> GetByIdAsync(string id) =>
            _db.Employees.Include(e => e.Account)
                         .FirstOrDefaultAsync(e => e.EmployeeId == id && !e.IsDeleted);

        public Task<Employee?> GetByAccountIdAsync(string accountId) =>
            _db.Employees.FirstOrDefaultAsync(e => e.AccountId == accountId && !e.IsDeleted);

        public Task<List<Employee>> SearchAsync(string keyword) =>
            _db.Employees
               .Include(e => e.Account)
               .Where(e => !e.IsDeleted && (
                   e.FullName.Contains(keyword) ||
                   e.PhoneNumber!.Contains(keyword) ||
                   e.Shift!.Contains(keyword)
               ))
               .ToListAsync();

        public Task<List<Employee>> GetDeletedAsync() =>
            _db.Employees.Where(e => e.IsDeleted).ToListAsync();

        public async Task AddAsync(Employee employee)
        {
            _db.Employees.Add(employee);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            _db.Employees.Update(employee);
            await _db.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(string id)
        {
            var e = await _db.Employees.FindAsync(id);
            if (e != null) { e.IsDeleted = true; await _db.SaveChangesAsync(); }
        }

        public async Task RestoreAsync(string id)
        {
            var e = await _db.Employees.FindAsync(id);
            if (e != null) { e.IsDeleted = false; await _db.SaveChangesAsync(); }
        }
    }

    // ── VehicleRepository ────────────────────────────────────
    public class VehicleRepository : IVehicleRepository
    {
        private readonly AppDbContext _db;
        public VehicleRepository(AppDbContext db) => _db = db;

        public Task<Vehicle?> GetByPlateAsync(string plate) =>
            _db.Vehicles.FirstOrDefaultAsync(v => v.VehiclePlate == plate);

        public Task<List<Vehicle>> GetByCustomerIdAsync(string customerId) =>
            _db.Vehicles.Where(v => v.CustomerId == customerId).ToListAsync();

        public Task<List<Vehicle>> GetAllAsync() =>
            _db.Vehicles.ToListAsync();

        public async Task AddAsync(Vehicle vehicle)
        {
            _db.Vehicles.Add(vehicle);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Vehicle vehicle)
        {
            _db.Vehicles.Update(vehicle);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string plate)
        {
            var v = await GetByPlateAsync(plate);
            if (v != null)
            {
                _db.Vehicles.Remove(v);
                await _db.SaveChangesAsync();
            }
        }

        public Task<bool> ExistsAsync(string plate) =>
            _db.Vehicles.AnyAsync(v => v.VehiclePlate == plate);
    }

    // ── ParkingSlotRepository ────────────────────────────────
    public class ParkingSlotRepository : IParkingSlotRepository
    {
        private readonly AppDbContext _db;
        public ParkingSlotRepository(AppDbContext db) => _db = db;

        public Task<ParkingSlot?> GetByIdAsync(string id) =>
            _db.ParkingSlots.FirstOrDefaultAsync(p => p.SlotId == id);

        public Task<List<ParkingSlot>> GetAllAsync() =>
            _db.ParkingSlots.ToListAsync();

        public Task<List<ParkingSlot>> GetAvailableAsync(string vehicleType) =>
            _db.ParkingSlots
               .Where(p => p.VehicleType == vehicleType && p.Status == "Trống")
               .ToListAsync();

        public async Task UpdateAsync(ParkingSlot slot)
        {
            _db.ParkingSlots.Update(slot);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(string slotId, string status)
        {
            var slot = await GetByIdAsync(slotId);
            if (slot != null)
            {
                slot.Status = status;
                slot.LastUpdated = DateTime.Now;
                await UpdateAsync(slot);
            }
        }

        public Task<int> GetAvailableCountAsync(string lotId, string vehicleType) =>
            _db.ParkingSlots
               .Where(p => p.VehicleType == vehicleType && p.Status == "Trống")
               .CountAsync();
    }

    // ── TicketRepository ─────────────────────────────────────
    public class TicketRepository : ITicketRepository
    {
        private readonly AppDbContext _db;
        public TicketRepository(AppDbContext db) => _db = db;

        public Task<Ticket?> GetByIdAsync(string id) =>
            _db.Tickets.Include(t => t.Customer)
                       .Include(t => t.ParkingSlot)
                       .FirstOrDefaultAsync(t => t.TicketId == id);

        public Task<List<Ticket>> GetByCustomerIdAsync(string customerId) =>
            _db.Tickets.Where(t => t.CustomerId == customerId).ToListAsync();

        public Task<List<Ticket>> GetAllAsync() =>
            _db.Tickets.ToListAsync();

        public Task<Ticket?> GetActiveByPlateAsync(string plate) =>
            _db.Tickets.FirstOrDefaultAsync(t => t.VehiclePlate == plate && t.Status == "Đang trong bãi");

        public Task<List<Ticket>> GetActiveTicketsAsync() =>
            _db.Tickets.Where(t => t.Status == "Đang trong bãi").ToListAsync();

        public async Task<string> GenerateIdAsync()
        {
            var lastId = await _db.Tickets.OrderByDescending(t => t.TicketId).FirstOrDefaultAsync();
            var num = lastId == null ? 1 : int.Parse(lastId.TicketId.Replace("TKT", "")) + 1;
            return $"TKT{num:D3}";
        }

        public async Task AddAsync(Ticket ticket)
        {
            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Ticket ticket)
        {
            _db.Tickets.Update(ticket);
            await _db.SaveChangesAsync();
        }
    }

    // ── MonthlyTicketRepository ──────────────────────────────
    public class MonthlyTicketRepository : IMonthlyTicketRepository
    {
        private readonly AppDbContext _db;
        public MonthlyTicketRepository(AppDbContext db) => _db = db;

        public Task<MonthlyTicket?> GetByIdAsync(string id) =>
            _db.MonthlyTickets.FirstOrDefaultAsync(m => m.MonthlyTicketId == id);

        public Task<List<MonthlyTicket>> GetByCustomerIdAsync(string customerId) =>
            _db.MonthlyTickets.Where(m => m.CustomerId == customerId).ToListAsync();

        public Task<List<MonthlyTicket>> GetAllAsync() =>
            _db.MonthlyTickets.ToListAsync();

        public Task<MonthlyTicket?> GetActiveByPlateAsync(string plate) =>
            _db.MonthlyTickets.FirstOrDefaultAsync(m => m.VehiclePlate == plate && m.Status == "Hoạt động");

        public Task<List<MonthlyTicket>> GetExpiringSoonAsync(int days) =>
            _db.MonthlyTickets
               .Where(m => m.Status == "Hoạt động" && m.EndDate <= DateTime.Now.AddDays(days))
               .ToListAsync();

        public async Task<string> GenerateIdAsync()
        {
            var lastId = await _db.MonthlyTickets.OrderByDescending(m => m.MonthlyTicketId).FirstOrDefaultAsync();
            var num = lastId == null ? 1 : int.Parse(lastId.MonthlyTicketId.Replace("MTK", "")) + 1;
            return $"MTK{num:D3}";
        }

        public async Task AddAsync(MonthlyTicket monthlyTicket)
        {
            _db.MonthlyTickets.Add(monthlyTicket);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(MonthlyTicket monthlyTicket)
        {
            _db.MonthlyTickets.Update(monthlyTicket);
            await _db.SaveChangesAsync();
        }
    }

    // ── ReservationRepository ────────────────────────────────
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _db;
        public ReservationRepository(AppDbContext db) => _db = db;

        public Task<Reservation?> GetByIdAsync(string id) =>
            _db.Reservations.Include(r => r.Customer)
                            .Include(r => r.ParkingSlot)
                            .FirstOrDefaultAsync(r => r.ReservationId == id);

        public Task<List<Reservation>> GetByCustomerIdAsync(string customerId) =>
            _db.Reservations.Include(r => r.Customer)
                            .Include(r => r.Vehicle)
                            .Include(r => r.ParkingSlot)
                            .Where(r => r.CustomerId == customerId)
                            .ToListAsync();

        public Task<List<Reservation>> GetAllAsync() =>
            _db.Reservations.ToListAsync();

        public Task<Reservation?> GetActiveByPlateAsync(string plate) =>
            _db.Reservations.FirstOrDefaultAsync(r => r.VehiclePlate == plate && r.Status == "Chờ");

        public async Task<string> GenerateIdAsync()
        {
            var lastId = await _db.Reservations.OrderByDescending(r => r.ReservationId).FirstOrDefaultAsync();
            var num = lastId == null ? 1 : int.Parse(lastId.ReservationId.Replace("RES", "")) + 1;
            return $"RES{num:D3}";
        }

        public async Task AddAsync(Reservation reservation)
        {
            _db.Reservations.Add(reservation);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Reservation reservation)
        {
            _db.Reservations.Update(reservation);
            await _db.SaveChangesAsync();
        }
    }

    // ── PaymentRepository ────────────────────────────────────
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _db;
        public PaymentRepository(AppDbContext db) => _db = db;

        public Task<Payment?> GetByTicketIdAsync(string ticketId) =>
            _db.Payments.FirstOrDefaultAsync(p => p.TicketId == ticketId);

        public Task<Payment?> GetByMonthlyTicketIdAsync(string monthlyTicketId) =>
            _db.Payments.FirstOrDefaultAsync(p => p.MonthlyTicketId == monthlyTicketId);

        public Task<List<Payment>> GetAllAsync() =>
            _db.Payments.ToListAsync();

        public Task<List<Payment>> GetByDateRangeAsync(DateTime from, DateTime to) =>
            _db.Payments
               .Where(p => p.PaymentTime >= from && p.PaymentTime <= to)
               .ToListAsync();

        public async Task<string> GenerateIdAsync()
        {
            var lastId = await _db.Payments.OrderByDescending(p => p.PaymentId).FirstOrDefaultAsync();
            var num = lastId == null ? 1 : int.Parse(lastId.PaymentId.Replace("PAY", "")) + 1;
            return $"PAY{num:D3}";
        }

        public async Task AddAsync(Payment payment)
        {
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();
        }
    }

    // ── ManagerRepository ────────────────────────────────────
    public class ManagerRepository : IManagerRepository
    {
        private readonly AppDbContext _db;
        public ManagerRepository(AppDbContext db) => _db = db;

        public Task<List<Manager>> GetAllAsync() =>
            _db.Managers.ToListAsync();

        public Task<Manager?> GetByIdAsync(string id) =>
            _db.Managers.FirstOrDefaultAsync(m => m.ManagerId == id);

        public async Task AddAsync(Manager manager)
        {
            _db.Managers.Add(manager);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Manager manager)
        {
            _db.Managers.Update(manager);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string id)
        {
            var m = await GetByIdAsync(id);
            if (m != null)
            {
                _db.Managers.Remove(m);
                await _db.SaveChangesAsync();
            }
        }
    }

    // ── EmployeeInviteRepository ─────────────────────────────
    public class EmployeeInviteRepository : IEmployeeInviteRepository
    {
        private readonly AppDbContext _db;
        public EmployeeInviteRepository(AppDbContext db) => _db = db;

        public Task<EmployeeInvite?> GetByTokenAsync(string token) =>
            _db.EmployeeInvites.FirstOrDefaultAsync(i => i.InviteToken == token);

        public Task<EmployeeInvite?> GetByEmailAsync(string email) =>
            _db.EmployeeInvites.FirstOrDefaultAsync(i => i.Email == email && !i.IsUsed);

        public Task<List<EmployeeInvite>> GetPendingAsync() =>
            _db.EmployeeInvites
               .Where(i => !i.IsUsed && i.ExpiryTime > DateTime.Now)
               .ToListAsync();

        public async Task AddAsync(EmployeeInvite invite)
        {
            _db.EmployeeInvites.Add(invite);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(EmployeeInvite invite)
        {
            _db.EmployeeInvites.Update(invite);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(string token)
        {
            var invite = await GetByTokenAsync(token);
            if (invite != null)
            {
                _db.EmployeeInvites.Remove(invite);
                await _db.SaveChangesAsync();
            }
        }
    }

    // ── ParkingSlotAuditLogRepository ────────────────────────
    public class ParkingSlotAuditLogRepository : IParkingSlotAuditLogRepository
    {
        private readonly AppDbContext _db;
        public ParkingSlotAuditLogRepository(AppDbContext db) => _db = db;

        public Task<List<ParkingSlotAuditLog>> GetBySlotIdAsync(string slotId) =>
            _db.ParkingSlotAuditLogs
               .Where(log => log.SlotId == slotId)
               .OrderByDescending(log => log.ChangedAt)
               .ToListAsync();

        public Task<List<ParkingSlotAuditLog>> GetByEmployeeIdAsync(string employeeId) =>
            _db.ParkingSlotAuditLogs
               .Where(log => log.EmployeeId == employeeId)
               .OrderByDescending(log => log.ChangedAt)
               .ToListAsync();

        public Task<List<ParkingSlotAuditLog>> GetByDateRangeAsync(DateTime from, DateTime to) =>
            _db.ParkingSlotAuditLogs
               .Where(log => log.ChangedAt >= from && log.ChangedAt <= to)
               .OrderByDescending(log => log.ChangedAt)
               .ToListAsync();

        public async Task<string> GenerateIdAsync()
        {
            var lastId = await _db.ParkingSlotAuditLogs.OrderByDescending(l => l.LogId).FirstOrDefaultAsync();
            var num = lastId == null ? 1 : int.Parse(lastId.LogId.Replace("LOG", "")) + 1;
            return $"LOG{num:D4}";
        }

        public async Task AddAsync(ParkingSlotAuditLog log)
        {
            _db.ParkingSlotAuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }

    // ── OtpRepository ───────────────────────────────────────
    public class OtpRepository : IOtpRepository
    {
        private readonly AppDbContext _db;
        public OtpRepository(AppDbContext db) => _db = db;

        public Task<Otp?> GetByIdAsync(string id) =>
            _db.Otps.FirstOrDefaultAsync(o => o.OtpId == id);

        public Task<Otp?> GetLatestByEmailAsync(string email) =>
            _db.Otps
               .Where(o => o.Email == email && !o.IsVerified && o.ExpiresAt > DateTime.UtcNow)
               .OrderByDescending(o => o.CreatedAt)
               .FirstOrDefaultAsync();

        public async Task<bool> IsValidOtpAsync(string email, string code)
        {
            var otp = await _db.Otps
                .FirstOrDefaultAsync(o => o.Email == email && 
                                         o.Code == code && 
                                         !o.IsVerified && 
                                         o.ExpiresAt > DateTime.UtcNow);
            return otp != null;
        }

        public async Task<string> GenerateIdAsync()
        {
            var lastId = await _db.Otps.OrderByDescending(o => o.OtpId).FirstOrDefaultAsync();
            var num = lastId == null ? 1 : int.Parse(lastId.OtpId.Replace("OTP", "")) + 1;
            return $"OTP{num:D6}";
        }

        public async Task AddAsync(Otp otp)
        {
            _db.Otps.Add(otp);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Otp otp)
        {
            _db.Otps.Update(otp);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteExpiredAsync()
        {
            var expired = await _db.Otps
                .Where(o => o.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            if (expired.Any())
            {
                _db.Otps.RemoveRange(expired);
                await _db.SaveChangesAsync();
            }
        }
    }

    // ── PricingRepository ────────────────────────────────────
    public class PricingRepository : IPricingRepository
    {
        private readonly AppDbContext _db;
        public PricingRepository(AppDbContext db) => _db = db;

        public Task<List<PricingConfiguration>> GetAllPricingConfigAsync() =>
            _db.PricingConfigurations.ToListAsync();

        public Task<PricingConfiguration?> GetPricingByTypeAndRateAsync(string vehicleType, string rateType) =>
            _db.PricingConfigurations
               .FirstOrDefaultAsync(p => p.VehicleType == vehicleType && p.RateType == rateType);

        public async Task UpsertPricingAsync(PricingConfiguration pricing)
        {
            _db.PricingConfigurations.Add(pricing);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAllPricingAsync()
        {
            var allPricing = await GetAllPricingConfigAsync();
            _db.PricingConfigurations.RemoveRange(allPricing);
            await _db.SaveChangesAsync();
        }
    }
}
