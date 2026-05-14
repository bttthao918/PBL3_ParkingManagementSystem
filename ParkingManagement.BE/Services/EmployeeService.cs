using BCrypt.Net;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;
using System.Text.RegularExpressions;
using System.Net.Mail;

namespace ParkingManagement.BLL.Services.Implementations
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repo;
        private readonly IAccountRepository _accountRepo;
        private readonly IEmployeeInviteRepository _inviteRepo;
        private readonly IParkingSlotAuditLogRepository _auditLogRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(
            IEmployeeRepository repo,
            IAccountRepository accountRepo,
            IEmployeeInviteRepository inviteRepo,
            IParkingSlotAuditLogRepository auditLogRepo,
            ITicketRepository ticketRepo,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<EmployeeService> logger)
        {
            _repo = repo;
            _accountRepo = accountRepo;
            _inviteRepo = inviteRepo;
            _auditLogRepo = auditLogRepo;
            _ticketRepo = ticketRepo;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
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

            var employeeId = await GenerateNextEmployeeCodeAsync();
            var employee = new DAL.Models.Employee
            {
                EmployeeId = employeeId,
                EmployeeCode = employeeId,
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
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.FullName) ||
                    string.IsNullOrWhiteSpace(request.PhoneNumber) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    return new CreateEmployeeInviteResultDto { Success = false, Message = "Vui lòng nhập đầy đủ các trường bắt buộc." };
                }

                if (!IsValidEmail(request.Email))
                    return new CreateEmployeeInviteResultDto { Success = false, Message = "Email không đúng định dạng." };

                var fullNameValidation = ValidateFullName(request.FullName);
                if (!fullNameValidation.IsValid)
                    return new CreateEmployeeInviteResultDto { Success = false, Message = fullNameValidation.ErrorMessage };

                var phoneValidation = ValidatePhoneNumber(request.PhoneNumber);
                if (!phoneValidation.IsValid)
                    return new CreateEmployeeInviteResultDto { Success = false, Message = phoneValidation.ErrorMessage };

                var passwordValidation = ValidateStrongPassword(request.Password);
                if (!passwordValidation.IsValid)
                    return new CreateEmployeeInviteResultDto { Success = false, Message = passwordValidation.ErrorMessage };

                if (request.Password != request.ConfirmPassword)
                    return new CreateEmployeeInviteResultDto { Success = false, Message = "Mật khẩu xác nhận không khớp." };

                var email = request.Email.Trim().ToLower();
                var existingInvite = await _inviteRepo.GetByEmailAsync(email);
                var existingAccount = await _accountRepo.GetByEmailAsync(email);
                var refreshedPendingInvite = existingInvite != null || existingAccount != null;

                Account account;
                if (existingAccount != null)
                {
                    if (existingAccount.Role != "Employee")
                    {
                        return new CreateEmployeeInviteResultDto
                        {
                            Success = false,
                            Message = "Email này đã thuộc về một tài khoản khác trong hệ thống."
                        };
                    }

                    var existingEmployee = await _repo.GetByAccountIdAsync(existingAccount.AccountId);
                    if (existingAccount.IsActive || existingEmployee != null)
                    {
                        return new CreateEmployeeInviteResultDto
                        {
                            Success = false,
                            Message = "Email này đã có tài khoản nhân viên đang hoạt động."
                        };
                    }

                    existingAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
                    existingAccount.IsActive = false;
                    existingAccount.RequirePasswordChange = false;
                    await _accountRepo.UpdateAsync(existingAccount);
                    account = existingAccount;
                }
                else
                {
                    var accountId = $"ACC{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                    account = new Account
                    {
                        AccountId = accountId,
                        Email = email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                        Role = "Employee",
                        IsActive = false, // Chỉ active sau khi nhân viên xác nhận email
                        RequirePasswordChange = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _accountRepo.AddAsync(account);
                }

                if (existingInvite != null && !existingInvite.IsUsed)
                {
                    await _inviteRepo.DeleteAsync(existingInvite.InviteToken);
                }

                var employeeCode = await GenerateNextEmployeeCodeAsync();

                var inviteToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    .Replace("/", "_")
                    .Replace("+", "-")
                    .Replace("=", string.Empty);
                var inviteExpiry = DateTime.UtcNow.AddDays(3);

                var invite = new EmployeeInvite
                {
                    InviteToken = inviteToken,
                    EmployeeCode = employeeCode,
                    Email = email,
                    FullName = request.FullName.Trim(),
                    PhoneNumber = request.PhoneNumber.Trim(),
                    Shift = request.Shift?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    ExpiryTime = inviteExpiry,
                    IsUsed = false
                };
                await _inviteRepo.AddAsync(invite);

                var emailSent = false;
                string? emailError = null;

                if (request.SendInvitationEmail)
                {
                    var backendBaseUrl = _configuration["BackendBaseUrl"] ?? "http://localhost:5188";
                    var confirmationUrl = $"{backendBaseUrl.TrimEnd('/')}/api/employees/invite/confirm?token={Uri.EscapeDataString(inviteToken)}";
                    try
                    {
                        await _emailService.SendEmployeeInviteConfirmationEmailAsync(email, request.FullName.Trim(), employeeCode, confirmationUrl, inviteExpiry);
                        emailSent = true;
                    }
                    catch (Exception mailEx)
                    {
                        emailError = mailEx.Message;
                        _logger.LogError(mailEx, "Employee invite email failed for {Email}.", email);
                    }
                }

                return new CreateEmployeeInviteResultDto
                {
                    Success = true,
                    Message = !request.SendInvitationEmail
                        ? refreshedPendingInvite
                            ? "Đã làm mới lời mời đang chờ xác nhận email."
                            : "Đã tạo lời mời nhân viên. Tài khoản đang chờ xác nhận email."
                        : emailSent
                            ? refreshedPendingInvite
                                ? "Đã gửi lại email xác nhận. Nhân viên sẽ được thêm vào hệ thống sau khi hoàn tất xác minh Gmail."
                                : "Đã tạo lời mời nhân viên. Email xác nhận đã được gửi; nhân viên sẽ được thêm vào hệ thống sau khi hoàn tất xác minh Gmail."
                            : $"Đã tạo lời mời nhân viên nhưng chưa gửi được email. Vui lòng kiểm tra cấu hình Gmail. Chi tiết: {emailError}",
                    EmployeeCode = employeeCode,
                    InviteToken = inviteToken,
                    InviteExpiry = inviteExpiry
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateEmployeeInviteAsync failed.");
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

            var employeeCode = await GenerateNextEmployeeCodeAsync();
            var inviteToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            var invite = new EmployeeInvite
            {
                InviteToken = inviteToken,
                EmployeeCode = employeeCode,
                Email = email,
                FullName = dto.FullName.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                Shift = dto.Shift?.Trim(),
                CreatedAt = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddHours(24),
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
            if (DateTime.UtcNow > invite.ExpiryTime) return ServiceResult<EmployeeInviteDto>.Fail("Link invite đã hết hạn.");

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
                if (string.IsNullOrWhiteSpace(request.InviteToken))
                    return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Thiếu token xác nhận." };

                var invite = await _inviteRepo.GetByTokenAsync(request.InviteToken);
                if (invite == null) return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Link invite không hợp lệ" };
                if (invite.IsUsed) return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Link invite này đã được sử dụng" };
                if (DateTime.UtcNow > invite.ExpiryTime) return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Link invite đã hết hạn" };

                var account = await _accountRepo.GetByEmailAsync(invite.Email);
                if (account == null || account.Role != "Employee")
                    return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Không tìm thấy tài khoản nhân viên chờ xác nhận." };

                var employee = await _repo.GetByAccountIdAsync(account.AccountId);
                if (employee == null)
                {
                    employee = new Employee
                    {
                        EmployeeId = invite.EmployeeCode,
                        EmployeeCode = invite.EmployeeCode,
                        AccountId = account.AccountId,
                        FullName = invite.FullName.Trim(),
                        PhoneNumber = invite.PhoneNumber?.Trim(),
                        Shift = invite.Shift?.Trim(),
                        IsDeleted = false
                    };
                    await _repo.AddAsync(employee);
                }
                invite.IsUsed = true;
                await _inviteRepo.UpdateAsync(invite);

                account.IsActive = true;
                await _accountRepo.UpdateAsync(account);

                return new ConfirmEmployeeInviteResultDto
                {
                    Success = true,
                    Message = "Xác minh Gmail thành công. Tài khoản đã được kích hoạt và nhân viên đã được thêm vào hệ thống.",
                    EmployeeCode = invite.EmployeeCode,
                    EmployeeId = employee.EmployeeId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConfirmInviteAsync failed.");
                return new ConfirmEmployeeInviteResultDto { Success = false, Message = $"Lỗi xác nhận invite: {ex.Message}" };
            }
        }

        private async Task<string> GenerateNextEmployeeCodeAsync()
        {
            var employees = await _repo.GetAllAsync(includeDeleted: true);
            var pendingInvites = await _inviteRepo.GetPendingAsync();

            var maxEmployeeNumber = employees
                .Select(e => ParseSequentialEmployeeNumber(e.EmployeeCode))
                .Where(number => number.HasValue)
                .Select(number => number!.Value)
                .DefaultIfEmpty(0)
                .Max();

            var maxPendingInviteNumber = pendingInvites
                .Select(invite => ParseSequentialEmployeeNumber(invite.EmployeeCode))
                .Where(number => number.HasValue)
                .Select(number => number!.Value)
                .DefaultIfEmpty(0)
                .Max();

            var nextNumber = Math.Max(maxEmployeeNumber, maxPendingInviteNumber) + 1;
            return $"EMP{nextNumber:D3}";
        }

        private static int? ParseSequentialEmployeeNumber(string? employeeCode)
        {
            if (string.IsNullOrWhiteSpace(employeeCode))
            {
                return null;
            }

            var match = Regex.Match(employeeCode.Trim(), @"^EMP(?<number>\d{3})$");
            return match.Success && int.TryParse(match.Groups["number"].Value, out var number)
                ? number
                : null;
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

        private (bool IsValid, string? ErrorMessage) ValidateStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return (false, "Mật khẩu không được để trống");
            if (password.Length < 8) return (false, "Mật khẩu phải ít nhất 8 ký tự");
            if (!Regex.IsMatch(password, @"[a-z]")) return (false, "Mật khẩu phải chứa ít nhất 1 chữ thường");
            if (!Regex.IsMatch(password, @"[A-Z]")) return (false, "Mật khẩu phải chứa ít nhất 1 chữ hoa");
            if (!Regex.IsMatch(password, @"[0-9]")) return (false, "Mật khẩu phải chứa chữ số (0-9)");
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':"",.<>?/\\|`~]")) return (false, "Mật khẩu phải chứa ký tự đặc biệt (!@#$%^&*...)");
            return (true, null);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var trimmed = email.Trim();
                var addr = new MailAddress(trimmed);
                return addr.Address.Equals(trimmed, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
