using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Data;

namespace ParkingManagement.Web.Controllers.Admin
{
    [Area("admin")]
    [Route("admin/employee")]
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _db;

        public EmployeeController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /admin/employee
        [HttpGet]
        [Route("")]
        [Route("index")]
        public async Task<IActionResult> Index()
        {
            var employees = await _db.Employees
                .Include(e => e.Account)
                .Where(e => e.Account.Role == "Employee" && e.Account.IsActive == true && e.IsDeleted == false)
                .OrderByDescending(e => e.EmployeeId)
                .ToListAsync();

            var deletedEmployees = await _db.Employees
                .Include(e => e.Account)
                .Where(e => e.Account.Role == "Employee" && (e.Account.IsActive == false || e.IsDeleted == true))
                .OrderByDescending(e => e.EmployeeId)
                .ToListAsync();

            ViewBag.Employees = employees;
            ViewBag.DeletedEmployees = deletedEmployees;

            return View();
        }

        // GET: /admin/employee/get/{id}
        [HttpGet]
        [Route("get/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var employee = await _db.Employees
                .Include(e => e.Account)
                .Where(e => e.Account.Role == "Employee")
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null)
                return Json(new { success = false });

            return Json(new
            {
                employee.EmployeeId,
                employee.FullName,
                employee.Account.Email,
                employee.PhoneNumber,
                Shift = employee.Shift ?? "Sáng",
                employee.EmployeeCode
            });
        }

        // POST: /admin/employee/create
        [HttpPost]
        [Route("create")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create([FromBody] EmployeeCreateModel model)
        {
            try
            {
                var existingUser = await _db.Accounts.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    return Json(new { success = false, message = "Email đã tồn tại!" });
                }

                string newAccountId = "ACC" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);
                
                var account = new Account
                {
                    AccountId = newAccountId,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    Role = "Employee",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };

                _db.Accounts.Add(account);

                string newEmployeeId = "EMP" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);
                
                var employee = new Employee
                {
                    EmployeeId = newEmployeeId,
                    EmployeeCode = "NV" + new Random().Next(1000, 9999), // or generate a better one
                    AccountId = account.AccountId,
                    FullName = model.FullName,
                    PhoneNumber = model.Phone,
                    Shift = null,
                    IsDeleted = false
                };

                _db.Employees.Add(employee);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm nhân viên thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // PUT: /admin/employee/update
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] EmployeeUpdateModel model)
        {
            try
            {
                var employee = await _db.Employees
                    .Include(e => e.Account)
                    .Where(e => e.Account.Role == "Employee")
                    .FirstOrDefaultAsync(e => e.EmployeeId == model.EmployeeId);

                if (employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên!" });
                }

                employee.FullName = model.FullName;
                employee.PhoneNumber = model.Phone;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    employee.Account.PasswordHash = HashPassword(model.Password);
                }

                employee.Shift = model.Shift;

                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // DELETE: /admin/employee/delete/{id}
        [HttpDelete]
        [Route("delete/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var employee = await _db.Employees
                    .Include(e => e.Account)
                    .Where(e => e.Account.Role == "Employee")
                    .FirstOrDefaultAsync(e => e.EmployeeId == id);

                if (employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên!" });
                }

                employee.IsDeleted = true;
                employee.Account.IsActive = false;
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Đã chuyển vào thùng rác!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: /admin/employee/restore/{id}
        [HttpPost]
        [Route("restore/{id}")]
        public async Task<IActionResult> Restore(string id)
        {
            try
            {
                var employee = await _db.Employees
                    .Include(e => e.Account)
                    .Where(e => e.Account.Role == "Employee")
                    .FirstOrDefaultAsync(e => e.EmployeeId == id);

                if (employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên!" });
                }

                employee.IsDeleted = false;
                employee.Account.IsActive = true;
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Khôi phục thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // DELETE: /admin/employee/permanent/{id}
        [HttpDelete]
        [Route("permanent/{id}")]
        public async Task<IActionResult> PermanentDelete(string id)
        {
            try
            {
                var employee = await _db.Employees
                    .Include(e => e.Account)
                    .Where(e => e.Account.Role == "Employee")
                    .FirstOrDefaultAsync(e => e.EmployeeId == id);

                if (employee == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nhân viên!" });
                }

                _db.Employees.Remove(employee);
                _db.Accounts.Remove(employee.Account);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Đã xóa vĩnh viễn!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /admin/employee/search
        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> Search(string keyword)
        {
            var employees = await _db.Employees
                .Include(e => e.Account)
                .Where(e => e.Account.Role == "Employee" && e.Account.IsActive == true && e.IsDeleted == false &&
                    (e.FullName.Contains(keyword) ||
                     e.Account.Email.Contains(keyword) ||
                     (e.PhoneNumber != null && e.PhoneNumber.Contains(keyword))))
                .Select(e => new
                {
                    e.EmployeeId,
                    e.FullName,
                    e.Account.Email,
                    e.PhoneNumber,
                    e.EmployeeCode,
                    Shift = e.Shift ?? "Sáng"
                })
                .ToListAsync();

            return Json(employees);
        }

        // Hàm mã hóa mật khẩu BCrypt
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }

    public class EmployeeCreateModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
    }

    public class EmployeeUpdateModel
    {
        public string EmployeeId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? Password { get; set; }
        public string Shift { get; set; } = "Sáng";
    }
}
