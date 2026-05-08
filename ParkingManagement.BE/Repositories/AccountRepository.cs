using Microsoft.EntityFrameworkCore;
using ParkingManagement.DAL.Data;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;

namespace ParkingManagement.DAL.Implementations
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _db;
        public AccountRepository(AppDbContext db) => _db = db;

        public Task<Account?> GetByIdAsync(string id) =>
            _db.Accounts.FindAsync(id).AsTask();

        public Task<Account?> GetByEmailAsync(string email) =>
            _db.Accounts.FirstOrDefaultAsync(a => a.Email == email);

        public async Task AddAsync(Account account)
        {
            // 1. Tạo ID một lần duy nhất, không dùng vòng lặp while để tránh treo
            if (string.IsNullOrEmpty(account.AccountId))
            {
                account.AccountId = "ACC" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            }

            // 2. Thêm vào context
            _db.Accounts.Add(account);

            // 3. Dùng try-catch để nếu có lỗi, Visual Studio sẽ chỉ đích danh lỗi gì
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Khi bị nhảy vào đây, hãy rê chuột vào biến 'ex' để xem thông báo lỗi (Message)
                throw new Exception("Lỗi Database: " + ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task UpdateAsync(Account account)
        {
            _db.Accounts.Update(account);
            await _db.SaveChangesAsync();
        }

        public Task<bool> ExistsEmailAsync(string email) =>
            _db.Accounts.AnyAsync(a => a.Email == email);
    }
}
