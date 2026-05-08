using ParkingManagement.DAL.Models;

namespace ParkingManagement.DAL.Interfaces
{
    public interface IAccountRepository
    {
        Task<Account?> GetByIdAsync(string id);
        Task<Account?> GetByEmailAsync(string email);
        Task AddAsync(Account account);
        Task UpdateAsync(Account account);
        Task<bool> ExistsEmailAsync(string email);
    }

    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync(bool includeDeleted = false);
        Task<Customer?> GetByIdAsync(string id);
        Task<Customer?> GetByAccountIdAsync(string accountId);
        Task<List<Customer>> SearchAsync(string keyword);
        Task<List<Customer>> SearchAdvancedAsync(string? fullName, string? phoneNumber, string? email, string? vehiclePlate, int maxResults = 50);
        Task<List<Customer>> GetDeletedAsync();
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task SoftDeleteAsync(string id);
        Task RestoreAsync(string id);
    }

    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync(bool includeDeleted = false);
        Task<Employee?> GetByIdAsync(string id);
        Task<Employee?> GetByAccountIdAsync(string accountId);
        Task<List<Employee>> SearchAsync(string keyword);
        Task<List<Employee>> GetDeletedAsync();
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task SoftDeleteAsync(string id);
        Task RestoreAsync(string id);
    }

    public interface IEmployeeInviteRepository
    {
        Task<EmployeeInvite?> GetByTokenAsync(string token);
        Task<EmployeeInvite?> GetByEmailAsync(string email);
        Task<List<EmployeeInvite>> GetPendingAsync();
        Task AddAsync(EmployeeInvite invite);
        Task UpdateAsync(EmployeeInvite invite);
        Task DeleteAsync(string token);
    }

    public interface IVehicleRepository
    {
        Task<Vehicle?> GetByPlateAsync(string plate);
        Task<List<Vehicle>> GetByCustomerIdAsync(string customerId);
        Task<List<Vehicle>> GetAllAsync();
        Task AddAsync(Vehicle vehicle);
        Task UpdateAsync(Vehicle vehicle);
        Task DeleteAsync(string plate);
        Task<bool> ExistsAsync(string plate);
    }

    public interface IParkingSlotRepository
    {
        Task<ParkingSlot?> GetByIdAsync(string id);
        Task<List<ParkingSlot>> GetAllAsync();
        Task<List<ParkingSlot>> GetAvailableAsync(string vehicleType);
        Task UpdateAsync(ParkingSlot slot);
        Task UpdateStatusAsync(string slotId, string status);
        Task<int> GetAvailableCountAsync(string lotId, string vehicleType);
    }

    public interface ITicketRepository
    {
        Task<Ticket?> GetByIdAsync(string id);
        Task<List<Ticket>> GetByCustomerIdAsync(string customerId);
        Task<List<Ticket>> GetAllAsync();
        Task<Ticket?> GetActiveByPlateAsync(string plate);
        Task<List<Ticket>> GetActiveTicketsAsync();
        Task<string> GenerateIdAsync();
        Task AddAsync(Ticket ticket);
        Task UpdateAsync(Ticket ticket);
    }

    public interface IMonthlyTicketRepository
    {
        Task<MonthlyTicket?> GetByIdAsync(string id);
        Task<List<MonthlyTicket>> GetByCustomerIdAsync(string customerId);
        Task<List<MonthlyTicket>> GetAllAsync();
        Task<MonthlyTicket?> GetActiveByPlateAsync(string plate);
        Task<List<MonthlyTicket>> GetExpiringSoonAsync(int days);
        Task<string> GenerateIdAsync();
        Task AddAsync(MonthlyTicket monthlyTicket);
        Task UpdateAsync(MonthlyTicket monthlyTicket);
    }

    public interface IReservationRepository
    {
        Task<Reservation?> GetByIdAsync(string id);
        Task<List<Reservation>> GetByCustomerIdAsync(string customerId);
        Task<List<Reservation>> GetAllAsync();
        Task<Reservation?> GetActiveByPlateAsync(string plate);
        Task<string> GenerateIdAsync();
        Task AddAsync(Reservation reservation);
        Task UpdateAsync(Reservation reservation);
    }

    public interface IPaymentRepository
    {
        Task<Payment?> GetByTicketIdAsync(string ticketId);
        Task<Payment?> GetByMonthlyTicketIdAsync(string monthlyTicketId);
        Task<List<Payment>> GetAllAsync();
        Task<List<Payment>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<string> GenerateIdAsync();
        Task AddAsync(Payment payment);
    }

    public interface IManagerRepository
    {
        Task<List<Manager>> GetAllAsync();
        Task<Manager?> GetByIdAsync(string id);
        Task AddAsync(Manager manager);
        Task UpdateAsync(Manager manager);
        Task DeleteAsync(string id);
    }

    public interface IParkingSlotAuditLogRepository
    {
        Task<List<ParkingSlotAuditLog>> GetBySlotIdAsync(string slotId);
        Task<List<ParkingSlotAuditLog>> GetByEmployeeIdAsync(string employeeId);
        Task<List<ParkingSlotAuditLog>> GetByDateRangeAsync(DateTime from, DateTime to);
        Task<string> GenerateIdAsync();
        Task AddAsync(ParkingSlotAuditLog log);
    }

    public interface IOtpRepository
    {
        Task<Otp?> GetByIdAsync(string id);
        Task<Otp?> GetLatestByEmailAsync(string email);
        Task<bool> IsValidOtpAsync(string email, string code);
        Task<string> GenerateIdAsync();
        Task AddAsync(Otp otp);
        Task UpdateAsync(Otp otp);
        Task DeleteExpiredAsync();
    }
}
