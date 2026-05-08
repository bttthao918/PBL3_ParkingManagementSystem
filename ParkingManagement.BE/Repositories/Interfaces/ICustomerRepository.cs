using ParkingManagement.DAL.Models;

namespace ParkingManagement.DAL.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync(bool includeDeleted = false);
        Task<Customer?> GetByIdAsync(string customerId);
        Task<Customer?> GetByAccountIdAsync(string accountId);
        Task<List<Customer>> SearchAsync(string keyword); // tên, SĐT, email
        Task AddAsync(Customer customer);
        Task UpdateAsync(Customer customer);
        Task SoftDeleteAsync(string customerId); // IsDeleted = true
        Task<bool> ExistsAsync(string customerId);
    }
}
