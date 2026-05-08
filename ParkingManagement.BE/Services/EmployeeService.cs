using BCrypt.Net;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;
using System.Text.RegularExpressions;

namespace ParkingManagement.BLL.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;
        private readonly IAccountRepository _accountRepo;
        private readonly IEmployeeInviteRepository _inviteRepo;
        private readonly IParkingSlotAuditLogRepository _auditLogRepo;
        private readonly ITicketRepository _ticketRepo;

        public EmployeeService(
            IEmployeeRepository repo,
            IAccountRepository accountRepo,
            IEmployeeInviteRepository inviteRepo,
            IParkingSlotAuditLogRepository auditLogRepo,
            ITicketRepository ticketRepo)
        {
            _repo = repo;
            _accountRepo = accountRepo;
            _inviteRepo = inviteRepo;
            _auditLogRepo = auditLogRepo;
            _ticketRepo = ticketRepo;
        }

        // ── 1. Basic Employee CRUD ──
        public async Task<List<EmployeeDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list.Select(MapToDto).ToList();
        }

        public async Task<EmployeeDto?> GetByIdAsync(string id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e == null ? null : MapToDto(e);
        }

        public async Task<List<EmployeeDto>> SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword)) return await GetAllAsync();
            var list = await _repo.SearchAsync(keyword.Trim());
            return list.Select(MapToDto).ToList();
        }

        public async Task<ServiceResult<string>> CreateAsync(CreateEmployeeDto dto)
        {
            var email = dto.Email.Trim().ToLower();
            if (await _accountRepo.ExistsEmailAsync(email))
                return ServiceResult<string>.Fail("Email này đã tồn tại.");

            var accountId = $"ACC{DateTime.Now.Ticks % 100000:D6}";
            var account = new Account
            {
                AccountId = accountId,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
                Role = "Employee",
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            await _accountRepo.AddAsync(account);

            var employeeId = $"EMP{DateTime.Now.Ticks % 100000:D6}";
            var employee = new DAL.Models.Employee
            {
                EmployeeId = employeeId,
                EmployeeCode = $"EMP{DateTime.Now.Ticks % 100000:D5}",
                AccountId = accountId,
                FullName = dto.FullName.Trim(),
                PhoneNumber = dto.PhoneNumber,
                Shift = null,
                IsDeleted = false
            };
            await _repo.AddAsync(employee);

            return ServiceResult<string>.Ok(employeeId, "Tạo nhân viên thành công.");
        }

        public async Task<ServiceResult<string>> SoftDeleteAsync(string id)
        {
            var e = await _repo.GetByIdAsync(id);
            if (e == null)
                return ServiceResult<string>.Fail("Không tìm thấy nhân viên.");

            if (!string.IsNullOrEmpty(e.Shift))
            {
                return ServiceResult<string>.Fail(
                    "Không thể xóa nhân viên này vì nhân viên hiện đang có ca làm. " +
                    "Vui lòng xóa lịch làm việc trước khi xóa nhân viên.");
            }

            await _repo.SoftDeleteAsync(id);
            return ServiceResult<string>.Ok(id, "Xóa nhân viên thành công.");
        }

        public async Task<List<EmployeeDto>> GetDeletedAsync()
        {
            var list = await _repo.GetDeletedAsync();
            return list.Select(MapToDto).ToList();
        }

        public async Task<ServiceResult<string>> RestoreAsync(string id)
        {
            await _repo.RestoreAsync(id);
            return ServiceResult<string>.Ok(id, "Khôi phục thành công.");
        }

        private static EmployeeDto MapToDto(DAL.Models.Employee e) => new()
        {
            EmployeeId = e.EmployeeId,
            FullName = e.FullName,
            PhoneNumber = e.PhoneNumber,
            Shift = e.Shift,
            Email = e.Account?.Email,
            IsManager = e.Manager != null,
            Department = e.Manager?.FullName ?? "N/A",
            IsDeleted = e.IsDeleted
        };

        // ── 2. Manager Employee Management ──
        public async Task<ListManagerEmployeeDto> GetEmployeesAsync(ManagerEmployeeFilterDto filter)
        {
            try
            {
                var allEmployees = (await _repo.GetAllAsync())
                    .Where(e => !e.IsDeleted)
                    .ToList();

                var filtered = allEmployees.AsEnumerable();

                if (!string.IsNullOrEmpty(filter.Status))
                {
                    if (filter.Status == "Hoạt động")
                        filtered = filtered.Where(e => !e.IsDeleted);
                    else if (filter.Status == "Vô hiệu hóa")
                        filtered = filtered.Where(e => e.IsDeleted);
                }

                if (!string.IsNullOrEmpty(filter.Shift))
                    filtered = filtered.Where(e => e.Shift == filter.Shift);

                if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
                {
                    var keyword = filter.SearchKeyword.Trim().ToLower();
                    filtered = filtered.Where(e =>
                        e.FullName.ToLower().Contains(keyword) ||
                        e.Account?.Email.ToLower().Contains(keyword) == true);
                }

                var sorted = filtered.OrderByDescending(e => e.EmployeeCode).ToList();

                var totalItems = sorted.Count;
                var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)filter.PageSize);
                var items = sorted
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var employeeDtos = items.Select(e => new ManagerEmployeeListDto
                {
                    EmployeeId = e.EmployeeId,
                    FullName = e.FullName,
                    Email = e.Account?.Email ?? "",
                    PhoneNumber = e.PhoneNumber ?? "",
                    Shift = e.Shift,
                    Status = e.IsDeleted ? "Vô hiệu hóa" : "Hoạt động",
                    CreatedAt = e.Account?.CreatedAt ?? DateTime.Now,
                    LastLoginAt = null
                }).ToList();

                var totalActive = sorted.Count(e => !e.IsDeleted);
                var totalInactive = sorted.Count(e => e.IsDeleted);

                return new ListManagerEmployeeDto
                {
                    Items = employeeDtos,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    TotalActive = totalActive,
                    TotalInactive = totalInactive
                };
            }
            catch (Exception)
            {
                return new ListManagerEmployeeDto
                {
                    Items = new(),
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = 0,
                    TotalPages = 0,
                    TotalActive = 0,
                    TotalInactive = 0
                };
            }
        }

        public async Task<ManagerEmployeeDetailDto> GetEmployeeDetailAsync(string employeeId)
        {
            try
            {
                var employee = await _repo.GetByIdAsync(employeeId);
                if (employee == null)
                    throw new Exception("Nhân viên không tồn tại");

                var allTickets = (await _ticketRepo.GetAllAsync()).ToList();

                var totalTickets = allTickets.Count();
                var todayTickets = allTickets.Count(t => t.CheckInTime.Date == DateTime.Now.Date);
                var thisMonthTickets = allTickets.Count(t => 
                    t.CheckInTime.Year == DateTime.Now.Year && 
                    t.CheckInTime.Month == DateTime.Now.Month);

                var firstWorkDay = employee.Account?.CreatedAt;

                var uniqueWorkDays = allTickets
                    .Select(t => t.CheckInTime.Date)
                    .Distinct()
                    .Count();

                return new ManagerEmployeeDetailDto
                {
                    EmployeeId = employee.EmployeeId,
                    FullName = employee.FullName,
                    Email = employee.Account?.Email ?? "",
                    PhoneNumber = employee.PhoneNumber ?? "",
                    Shift = employee.Shift,
                    Status = employee.IsDeleted ? "Vô hiệu hóa" : "Hoạt động",
                    CreatedAt = employee.Account?.CreatedAt ?? DateTime.Now,
                    LastLoginAt = null,
                    TotalTicketsProcessed = totalTickets,
                    TicketsProcessedToday = todayTickets,
                    TicketsProcessedThisMonth = thisMonthTickets,
                    FirstWorkDay = firstWorkDay,
                    WorkDaysCount = uniqueWorkDays
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy chi tiết nhân viên: {ex.Message}");
            }
        }

        public async Task<CreateEmployeeInviteResultDto> CreateEmployeeInviteAsync(CreateEmployeeInviteByManagerDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return new CreateEmployeeInviteResultDto { Success = false, Message = "Email không được để trống" };

                var email = request.Email.Trim().ToLower();
                var existingAccount = await _accountRepo.GetByEmailAsync(email);
                if (existingAccount != null)
                    return new CreateEmployeeInviteResultDto { Success = false, Message = "Email này đã được đăng ký" };

                var employeeCode = $"EMP{DateTime.Now.Ticks % 100000:D5}";
                var employeeId = $"EMP{DateTime.Now.Ticks % 1000000:D6}";

                var accountId = $"ACC{DateTime.Now.Ticks % 100000:D6}";
                var account = new Account
                {
                    AccountId = accountId,
                    Email = email,
                    PasswordHash = "",
                    Role = "Employee",
                    IsActive = false,
                    CreatedAt = DateTime.Now
                };
                await _accountRepo.AddAsync(account);

                var employee = new Employee
                {
                    EmployeeId = employeeId,
                    EmployeeCode = employeeCode,
                    AccountId = accountId,
                    FullName = "",
                    PhoneNumber = "",
                    Shift = request.Shift,
                    IsDeleted = false
                };
                await _repo.AddAsync(employee);

                var inviteToken = Guid.NewGuid().ToString();
                var inviteExpiry = DateTime.Now.AddDays(7);

                return new CreateEmployeeInviteResultDto
                {
                    Success = true,
                    Message = "Tạo invite thành công. Vui lòng gửi email cho nhân viên.",
                    EmployeeCode = employeeCode,
                    InviteToken = inviteToken,
                    InviteExpiry = inviteExpiry
                };
            }
            catch (Exception ex)
            {
                return new CreateEmployeeInviteResultDto { Success = false, Message = $"Lỗi tạo invite: {ex.Message}" };
            }
        }

        public async Task<UpdateEmployeeResultDto> UpdateEmployeeAsync(string employeeId, UpdateEmployeeByManagerDto request)
        {
            try
            {
                var employee = await _repo.GetByIdAsync(employeeId);
                if (employee == null)
                    return new UpdateEmployeeResultDto { Success = false, Message = "Nhân viên không tồn tại" };

                if (!string.IsNullOrWhiteSpace(request.FullName))
                    employee.FullName = request.FullName.Trim();

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    employee.PhoneNumber = request.PhoneNumber.Trim();

                if (!string.IsNullOrEmpty(request.Shift))
                    employee.Shift = request.Shift;

                if (!string.IsNullOrEmpty(request.Status))
                {
                    if (request.Status == "Vô hiệu hóa")
                        employee.IsDeleted = true;
                    else if (request.Status == "Hoạt động")
                        employee.IsDeleted = false;
                }

                await _repo.UpdateAsync(employee);

                return new UpdateEmployeeResultDto { Success = true, Message = "Cập nhật thông tin nhân viên thành công" };
            }
            catch (Exception ex)
            {
                return new UpdateEmployeeResultDto { Success = false, Message = $"Lỗi cập nhật nhân viên: {ex.Message}" };
            }
        }

        public async Task<DeleteEmployeeResultDto> DeleteEmployeeAsync(DeleteEmployeeDto request)
        {
            try
            {
                var employee = await _repo.GetByIdAsync(request.EmployeeId);
                if (employee == null)
                    return new DeleteEmployeeResultDto { Success = false, Message = "Nhân viên không tồn tại" };

                employee.IsDeleted = true;
                if (employee.Account != null) employee.Account.IsActive = false;

                await _repo.UpdateAsync(employee);

                return new DeleteEmployeeResultDto
                {
                    Success = true,
                    Message = "Vô hiệu hóa nhân viên thành công",
                    EmployeeId = request.EmployeeId,
                    NewStatus = "Vô hiệu hóa"
                };
            }
            catch (Exception ex)
            {
                return new DeleteEmployeeResultDto { Success = false, Message = $"Lỗi vô hiệu hóa nhân viên: {ex.Message}", EmployeeId = request.EmployeeId };
            }
        }

        // ── 3. Employee Invite Processing ──
        public async Task<ServiceResult<EmployeeInviteDto>> CreateInviteAsync(CreateEmployeeInviteDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return ServiceResult<EmployeeInviteDto>.Fail("Vui lòng nhập đầy đủ thông tin.");

            var email = dto.Email.Trim().ToLower();
            var existingInvite = await _inviteRepo.GetByEmailAsync(email);
            if (existingInvite != null && !existingInvite.IsUsed)
                return ServiceResult<EmployeeInviteDto>.Fail("Email này đã có lời mời chưa sử dụng.");

            var employeeCode = $"EMP{DateTime.Now.Ticks % 100000:D6}";
            var inviteToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            var invite = new EmployeeInvite
            {
                InviteToken = inviteToken,
                EmployeeCode = employeeCode,
                Email = email,
                FullName = dto.FullName.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Shift = dto.Shift?.Trim(),
                CreatedAt = DateTime.Now,
                ExpiryTime = DateTime.Now.AddHours(24),
                IsUsed = false
            };

            await _inviteRepo.AddAsync(invite);

            var result = new EmployeeInviteDto
            {
                EmployeeCode = employeeCode,
                Email = email,
                FullName = dto.FullName,
                InviteToken = inviteToken,
                InviteExpiry = invite.ExpiryTime
            };

            return ServiceResult<EmployeeInviteDto>.Ok(result, "Tạo lời mời thành công.");
        }

        public async Task<ServiceResult<EmployeeInviteDto>> GetInviteByTokenAsync(string token)
        {
            var invite = await _inviteRepo.GetByTokenAsync(token);
            if (invite == null) return ServiceResult<EmployeeInviteDto>.Fail("Link invite không hợp lệ.");
            if (invite.IsUsed) return ServiceResult<EmployeeInviteDto>.Fail("Link invite này đã được sử dụng.");
            if (DateTime.Now > invite.ExpiryTime) return ServiceResult<EmployeeInviteDto>.Fail("Link invite đã hết hạn.");

            var dto = new EmployeeInviteDto
            {
                EmployeeCode = invite.EmployeeCode,
                Email = invite.Email,
                FullName = invite.FullName,
                InviteToken = invite.InviteToken,
                InviteExpiry = invite.ExpiryTime
            };

            return ServiceResult<EmployeeInviteDto>.Ok(dto, "Lấy thông tin thành công.");
        }

        public async Task<ConfirmEmployeeInviteResultDto> ConfirmInviteAsync(ConfirmEmployeeInviteDto request)
        {
            try
            {
                var fullNameValidation = ValidateFullName(request.FullName);
                if (!fullNameValidation.IsValid) return new ConfirmEmployeeInviteResultDto { Success = false, Message = fullNameValidation.ErrorMessage };

                var phoneValidation = ValidatePhoneNumber(request.PhoneNumber);
                if (!phoneValidation.IsValid) return new ConfirmEmployeeInviteResultDto { Success = false, Message = phoneValidation.ErrorMessage };

                var passwordValidation = ValidatePassword(request.Password);
                if (!passwordValidation.IsValid) return new ConfirmEmployeeInviteResultDto { Success = false, Message = passwordValidation.ErrorMessage };

                if (request.Password != request.ConfirmPassword) return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Mật khẩu xác nhận không khớp" };

                var invite = await _inviteRepo.GetByTokenAsync(request.InviteToken);
                if (invite == null) return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Link invite không hợp lệ" };
                if (invite.IsUsed) return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Link invite này đã được sử dụng" };
                if (DateTime.Now > invite.ExpiryTime) return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Link invite đã hết hạn" };

                invite.IsUsed = true;
                await _inviteRepo.UpdateAsync(invite);

                return new ConfirmEmployeeInviteResultDto
                {
                    Success = true,
                    Message = "Xác nhận invite thành công! Tài khoản của bạn đã được kích hoạt.",
                    EmployeeCode = invite.EmployeeCode,
                    EmployeeId = null
                };
            }
            catch (Exception ex)
            {
                return new ConfirmEmployeeInviteResultDto { Success = false, Message = $"Lỗi xác nhận invite: {ex.Message}" };
            }
        }

        private (bool IsValid, string? ErrorMessage) ValidateFullName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return (false, "Họ tên không được để trống");
            var trimmed = fullName.Trim();
            if (trimmed.Length < 3) return (false, "Họ tên phải ít nhất 3 ký tự");
            if (trimmed.Length > 100) return (false, "Họ tên không được vượt quá 100 ký tự");
            return (true, null);
        }

        private (bool IsValid, string? ErrorMessage) ValidatePhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return (false, "Số điện thoại không được để trống");
            var cleaned = Regex.Replace(phoneNumber.Trim(), @"[^\d]", "");
            if (cleaned.Length < 10) return (false, "Số điện thoại phải ít nhất 10 chữ số");
            if (cleaned.Length > 15) return (false, "Số điện thoại không được vượt quá 15 chữ số");
            return (true, null);
        }

        private (bool IsValid, string? ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return (false, "Mật khẩu không được để trống");
            if (password.Length < 6) return (false, "Mật khẩu phải ít nhất 6 ký tự");
            if (!Regex.IsMatch(password, @"[a-zA-Z]")) return (false, "Mật khẩu phải chứa chữ cái (a-z hoặc A-Z)");
            if (!Regex.IsMatch(password, @"[0-9]")) return (false, "Mật khẩu phải chứa chữ số (0-9)");
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':"",.<>?/\\|`~]")) return (false, "Mật khẩu phải chứa ký tự đặc biệt (!@#$%^&*...)");
            return (true, null);
        }
    }
}
