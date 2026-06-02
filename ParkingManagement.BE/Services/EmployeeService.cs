using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.BLL.Constants;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Data;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
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
        private readonly AppDbContext _db;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IConfiguration _configuration;
        private const string ActiveStatus = "Hoạt động";
        private const string DisabledStatus = "Vô hiệu hóa";
        private const string DeletedStatus = "Đã xóa";

        public EmployeeService(
            IEmployeeRepository repo,
            IAccountRepository accountRepo,
            IEmployeeInviteRepository inviteRepo,
            IParkingSlotAuditLogRepository auditLogRepo,
            ITicketRepository ticketRepo,
            IEmailService emailService,
            AppDbContext db,
            ILogger<EmployeeService> logger,
            IConfiguration configuration)
        {
            _repo = repo;
            _accountRepo = accountRepo;
            _inviteRepo = inviteRepo;
            _auditLogRepo = auditLogRepo;
            _ticketRepo = ticketRepo;
            _emailService = emailService;
            _db = db;
            _logger = logger;
            _configuration = configuration;
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

            e.IsDeleted = true;
            if (e.Account != null)
            {
                e.Account.IsActive = false;
            }

            await _repo.UpdateAsync(e);
            return ServiceResult<string>.Ok(id, "Xóa nhân viên thành công.");
        }

        public async Task<List<EmployeeDto>> GetDeletedAsync()
        {
            var list = await _repo.GetDeletedAsync();
            return list.Select(MapToDto).ToList();
        }

        public async Task<ServiceResult<string>> RestoreAsync(string id)
        {
            var employee = (await _repo.GetAllAsync(includeDeleted: true))
                .FirstOrDefault(e => e.EmployeeId == id);

            if (employee == null)
            {
                return ServiceResult<string>.Fail("Không tìm thấy nhân viên.");
            }

            employee.IsDeleted = false;
            if (employee.Account != null)
            {
                employee.Account.IsActive = true;
            }

            await _repo.UpdateAsync(employee);
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
                var allEmployees = await _repo.GetAllAsync(includeDeleted: true);

                var filtered = allEmployees.AsEnumerable();

                if (!string.IsNullOrEmpty(filter.Shift))
                    filtered = filtered.Where(e => e.Shift == filter.Shift);

                if (!string.IsNullOrWhiteSpace(filter.SearchKeyword))
                {
                    var keyword = filter.SearchKeyword.Trim().ToLower();
                    filtered = filtered.Where(e =>
                        e.FullName.ToLower().Contains(keyword) ||
                        e.Account?.Email.ToLower().Contains(keyword) == true);
                }

                var totalActive = filtered.Count(e => !e.IsDeleted && e.Account?.IsActive == true);
                var totalInactive = filtered.Count(e => !e.IsDeleted && e.Account?.IsActive != true);
                var totalDeleted = filtered.Count(e => e.IsDeleted);

                if (IsDeletedStatus(filter.Status))
                {
                    filtered = filtered.Where(e => e.IsDeleted);
                }
                else if (IsDisabledStatus(filter.Status))
                {
                    filtered = filtered.Where(e => !e.IsDeleted && e.Account?.IsActive != true);
                }
                else if (IsActiveStatus(filter.Status))
                {
                    filtered = filtered.Where(e => !e.IsDeleted && e.Account?.IsActive == true);
                }
                else
                {
                    filtered = filtered.Where(e => !e.IsDeleted);
                }

                var sorted = filtered.OrderByDescending(e => e.EmployeeCode).ToList();

                var totalItems = sorted.Count;
                var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)filter.PageSize);
                var items = sorted
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToList();

                var itemEmployeeIds = items.Select(e => e.EmployeeId).ToList();
                var ticketStatsByEmployee = await BuildTicketStatsByEmployeeAsync(itemEmployeeIds);
                var workMinutesByEmployee = await _db.WorkLogs
                    .Where(w => itemEmployeeIds.Contains(w.EmployeeId) && w.TotalMinutes.HasValue)
                    .GroupBy(w => w.EmployeeId)
                    .Select(g => new
                    {
                        EmployeeId = g.Key,
                        TotalWorkMinutes = g.Sum(w => w.TotalMinutes ?? 0)
                    })
                    .ToDictionaryAsync(x => x.EmployeeId, x => x.TotalWorkMinutes);

                var employeeDtos = items.Select(e =>
                {
                    var ticketStats = ticketStatsByEmployee.GetValueOrDefault(e.EmployeeId) ?? new EmployeeTicketStats();
                    return new ManagerEmployeeListDto
                    {
                        EmployeeId = e.EmployeeId,
                        EmployeeCode = e.EmployeeCode,
                        FullName = e.FullName,
                        Email = e.Account?.Email ?? "",
                        PhoneNumber = e.PhoneNumber ?? "",
                        Shift = e.Shift,
                        Status = GetManagerStatus(e),
                        CreatedAt = e.Account?.CreatedAt ?? DateTime.Now,
                        LastLoginAt = null,
                        TotalWorkMinutes = workMinutesByEmployee.GetValueOrDefault(e.EmployeeId),
                        TotalTicketsProcessed = ticketStats.TotalTicketsProcessed,
                        TicketsProcessedToday = ticketStats.TicketsProcessedToday,
                        TicketsProcessedThisMonth = ticketStats.TicketsProcessedThisMonth
                    };
                }).ToList();

                return new ListManagerEmployeeDto
                {
                    Items = employeeDtos,
                    PageNumber = filter.PageNumber,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    TotalActive = totalActive,
                    TotalInactive = totalInactive,
                    TotalDeleted = totalDeleted
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
                    TotalInactive = 0,
                    TotalDeleted = 0
                };
            }
        }

        public async Task<ManagerEmployeeDetailDto> GetEmployeeDetailAsync(string employeeId)
        {
            try
            {
                var employee = (await _repo.GetAllAsync(includeDeleted: true))
                    .FirstOrDefault(e => e.EmployeeId == employeeId);
                if (employee == null)
                    throw new Exception("Nhân viên không tồn tại");

                var ticketStats = (await BuildTicketStatsByEmployeeAsync(new[] { employeeId }))
                    .GetValueOrDefault(employeeId) ?? new EmployeeTicketStats();
                var completedWorkLogs = await _db.WorkLogs
                    .Where(w => w.EmployeeId == employeeId && w.TotalMinutes.HasValue)
                    .ToListAsync();

                var firstWorkDay = completedWorkLogs.Count > 0
                    ? completedWorkLogs.Min(w => w.WorkDate)
                    : employee.Account?.CreatedAt;

                var uniqueWorkDays = completedWorkLogs
                    .Select(w => w.WorkDate.Date)
                    .Distinct()
                    .Count();
                var totalWorkMinutes = completedWorkLogs.Sum(w => w.TotalMinutes ?? 0);

                return new ManagerEmployeeDetailDto
                {
                    EmployeeId = employee.EmployeeId,
                    EmployeeCode = employee.EmployeeCode,
                    FullName = employee.FullName,
                    Email = employee.Account?.Email ?? "",
                    PhoneNumber = employee.PhoneNumber ?? "",
                    Shift = employee.Shift,
                    Status = GetManagerStatus(employee),
                    CreatedAt = employee.Account?.CreatedAt ?? DateTime.Now,
                    LastLoginAt = null,
                    TotalTicketsProcessed = ticketStats.TotalTicketsProcessed,
                    TicketsProcessedToday = ticketStats.TicketsProcessedToday,
                    TicketsProcessedThisMonth = ticketStats.TicketsProcessedThisMonth,
                    FirstWorkDay = firstWorkDay,
                    WorkDaysCount = uniqueWorkDays,
                    TotalWorkMinutes = totalWorkMinutes
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy chi tiết nhân viên: {ex.Message}");
            }
        }

        private async Task<Dictionary<string, EmployeeTicketStats>> BuildTicketStatsByEmployeeAsync(IEnumerable<string> employeeIds)
        {
            var employeeIdSet = employeeIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var result = employeeIdSet.ToDictionary(
                id => id,
                _ => new EmployeeTicketStats(),
                StringComparer.OrdinalIgnoreCase);

            if (employeeIdSet.Count == 0)
                return result;

            var ticketPayments = (await _db.Payments
                    .Where(p => p.TicketId != null)
                    .ToListAsync())
                .Where(p => PaymentStatuses.IsSuccessful(p.Status))
                .ToList();

            var hasExplicitAttribution = ticketPayments
                .Any(p => !string.IsNullOrWhiteSpace(p.CollectedByEmployeeId));

            var workLogs = await _db.WorkLogs
                .Where(w => employeeIdSet.Contains(w.EmployeeId))
                .ToListAsync();

            foreach (var payment in ticketPayments)
            {
                var assignedEmployeeId = ResolvePaymentEmployeeId(payment, employeeIdSet, workLogs);
                if (assignedEmployeeId == null)
                    continue;

                result[assignedEmployeeId].Add(payment.PaymentTime);
            }

            if (!hasExplicitAttribution && !result.Values.Any(s => s.TotalTicketsProcessed > 0))
            {
                var fallbackStats = new EmployeeTicketStats();
                var tickets = await _db.Tickets.ToListAsync();
                foreach (var ticket in tickets)
                {
                    fallbackStats.Add(ticket.CheckOutTime ?? ticket.CheckInTime);
                }

                foreach (var employeeId in employeeIdSet)
                {
                    result[employeeId] = fallbackStats.Clone();
                }
            }

            return result;
        }

        private static string? ResolvePaymentEmployeeId(
            Payment payment,
            HashSet<string> employeeIdSet,
            List<WorkLog> workLogs)
        {
            if (!string.IsNullOrWhiteSpace(payment.CollectedByEmployeeId) &&
                employeeIdSet.Contains(payment.CollectedByEmployeeId))
            {
                return payment.CollectedByEmployeeId;
            }

            return workLogs
                .Where(w => w.StartTime <= payment.PaymentTime && GetWorkLogEndTime(w) >= payment.PaymentTime)
                .OrderByDescending(w => w.StartTime)
                .FirstOrDefault()
                ?.EmployeeId;
        }

        private static DateTime GetWorkLogEndTime(WorkLog log)
        {
            if (log.EndTime.HasValue)
                return log.EndTime.Value;

            if (log.TotalMinutes.HasValue)
                return log.StartTime.AddMinutes(Math.Max(0, log.TotalMinutes.Value));

            return log.StartTime.AddHours(12);
        }

        private sealed class EmployeeTicketStats
        {
            public int TotalTicketsProcessed { get; private set; }
            public int TicketsProcessedToday { get; private set; }
            public int TicketsProcessedThisMonth { get; private set; }

            public void Add(DateTime occurredAt)
            {
                var today = DateTime.Now.Date;
                TotalTicketsProcessed++;

                if (occurredAt.Date == today)
                    TicketsProcessedToday++;

                if (occurredAt.Year == today.Year && occurredAt.Month == today.Month)
                    TicketsProcessedThisMonth++;
            }

            public EmployeeTicketStats Clone()
            {
                return new EmployeeTicketStats
                {
                    TotalTicketsProcessed = TotalTicketsProcessed,
                    TicketsProcessedToday = TicketsProcessedToday,
                    TicketsProcessedThisMonth = TicketsProcessedThisMonth
                };
            }
        }

        public async Task<CreateEmployeeInviteResultDto> CreateEmployeeInviteAsync(CreateEmployeeInviteByManagerDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.PhoneNumber) &&
                    string.IsNullOrWhiteSpace(request.Password) &&
                    string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    return await CreateEmployeeInvitationOnlyAsync(request);
                }

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

                    var existingEmployee = (await _repo.GetAllAsync(includeDeleted: true))
                        .FirstOrDefault(e => e.AccountId == existingAccount.AccountId);
                    if (existingAccount.IsActive && existingEmployee is { IsDeleted: false })
                    {
                        return new CreateEmployeeInviteResultDto
                        {
                            Success = false,
                            Message = "Email này đã có tài khoản nhân viên đang hoạt động."
                        };
                    }

                    existingAccount.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
                    existingAccount.IsActive = true;
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
                        IsActive = true,
                        RequirePasswordChange = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _accountRepo.AddAsync(account);
                }

                if (existingInvite != null && !existingInvite.IsUsed)
                {
                    await _inviteRepo.DeleteAsync(existingInvite.InviteToken);
                }

                var employee = (await _repo.GetAllAsync(includeDeleted: true))
                    .FirstOrDefault(e => e.AccountId == account.AccountId);
                if (employee == null)
                {
                    var employeeCode = await GenerateNextEmployeeCodeAsync();
                    employee = new Employee
                    {
                        EmployeeId = employeeCode,
                        EmployeeCode = employeeCode,
                        AccountId = account.AccountId,
                        FullName = request.FullName.Trim(),
                        PhoneNumber = request.PhoneNumber.Trim(),
                        Shift = request.Shift?.Trim(),
                        IsDeleted = false
                    };
                    await _repo.AddAsync(employee);
                }
                else
                {
                    employee.FullName = request.FullName.Trim();
                    employee.PhoneNumber = request.PhoneNumber.Trim();
                    employee.Shift = request.Shift?.Trim();
                    employee.IsDeleted = false;
                    await _repo.UpdateAsync(employee);
                }

                var emailSent = false;
                string? emailError = null;

                if (request.SendInvitationEmail)
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            email,
                            "Tài khoản nhân viên ParkSmart đã được tạo",
                            $@"
                            <div style='font-family: Segoe UI, Tahoma, Geneva, Verdana, sans-serif; max-width: 560px; margin: auto; border: 1px solid #e0e0e0; border-radius: 10px; overflow: hidden;'>
                                <div style='background-color: #1e88e5; padding: 20px; text-align: center;'>
                                    <h2 style='color: white; margin: 0;'>ParkSmart Employee Account</h2>
                                </div>
                                <div style='padding: 24px; line-height: 1.6; color: #333;'>
                                    <p>Xin chào <strong>{request.FullName.Trim()}</strong>,</p>
                                    <p>Quản lý đã tạo tài khoản nhân viên cho bạn trên hệ thống ParkSmart.</p>
                                    <p>Mã nhân viên của bạn: <strong>{employee.EmployeeCode}</strong></p>
                                    <p>Tài khoản đã được kích hoạt. Bạn có thể đăng nhập bằng email này và mật khẩu do quản lý cung cấp.</p>
                                </div>
                            </div>");
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
                        ? "Đã tạo nhân viên. Tài khoản đã được kích hoạt và có thể đăng nhập ngay."
                        : emailSent
                            ? "Đã tạo nhân viên và gửi email thông báo. Tài khoản đã được kích hoạt và có thể đăng nhập ngay."
                            : $"Đã tạo nhân viên và kích hoạt tài khoản nhưng chưa gửi được email thông báo. Chi tiết: {emailError}",
                    EmployeeCode = employee.EmployeeCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateEmployeeInviteAsync failed.");
                return new CreateEmployeeInviteResultDto { Success = false, Message = $"Lỗi tạo invite: {ex.Message}" };
            }
        }

        private async Task<CreateEmployeeInviteResultDto> CreateEmployeeInvitationOnlyAsync(CreateEmployeeInviteByManagerDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.FullName))
            {
                return new CreateEmployeeInviteResultDto
                {
                    Success = false,
                    Message = "Vui lòng nhập tên và email nhân viên."
                };
            }

            if (!IsValidEmail(request.Email))
            {
                return new CreateEmployeeInviteResultDto
                {
                    Success = false,
                    Message = "Email không đúng định dạng."
                };
            }

            var fullNameValidation = ValidateFullName(request.FullName);
            if (!fullNameValidation.IsValid)
            {
                return new CreateEmployeeInviteResultDto
                {
                    Success = false,
                    Message = fullNameValidation.ErrorMessage
                };
            }

            var email = request.Email.Trim().ToLowerInvariant();
            var fullName = request.FullName.Trim();
            var existingInvite = await _inviteRepo.GetByEmailAsync(email);
            var existingAccount = await _accountRepo.GetByEmailAsync(email);
            var existingEmployee = existingAccount == null
                ? null
                : (await _repo.GetAllAsync(includeDeleted: true))
                    .FirstOrDefault(e => e.AccountId == existingAccount.AccountId);

            if (existingAccount != null && !string.Equals(existingAccount.Role, "Employee", StringComparison.OrdinalIgnoreCase))
            {
                return new CreateEmployeeInviteResultDto
                {
                    Success = false,
                    Message = "Email này đã thuộc về một tài khoản khác trong hệ thống."
                };
            }

            if (existingAccount?.IsActive == true && existingEmployee is { IsDeleted: false })
            {
                return new CreateEmployeeInviteResultDto
                {
                    Success = false,
                    Message = "Email này đã có tài khoản nhân viên đang hoạt động."
                };
            }

            if (existingInvite != null && !existingInvite.IsUsed)
            {
                await _inviteRepo.DeleteAsync(existingInvite.InviteToken);
            }

            var employeeCode = existingEmployee?.EmployeeCode ?? await GenerateNextEmployeeCodeAsync();
            var inviteToken = Guid.NewGuid().ToString("N");
            var inviteExpiry = DateTime.UtcNow.AddHours(24);

            var invite = new EmployeeInvite
            {
                InviteToken = inviteToken,
                EmployeeCode = employeeCode,
                Email = email,
                FullName = fullName,
                PhoneNumber = null,
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
                var inviteUrl = BuildEmployeeInviteUrl(inviteToken);
                try
                {
                    await _emailService.SendEmailAsync(
                        email,
                        "Mời hoàn tất tài khoản nhân viên ParkSmart",
                        $@"
                        <div style='font-family: Segoe UI, Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: auto; border: 1px solid #e5e7eb; border-radius: 14px; overflow: hidden;'>
                            <div style='background: #0f766e; padding: 22px 26px; color: white;'>
                                <h2 style='margin: 0; font-size: 22px;'>ParkSmart</h2>
                                <p style='margin: 6px 0 0;'>Lời mời hoàn tất tài khoản nhân viên</p>
                            </div>
                            <div style='padding: 26px; color: #1f2937; line-height: 1.6;'>
                                <p>Xin chào <strong>{fullName}</strong>,</p>
                                <p>Quản lý đã tạo lời mời nhân viên cho bạn trên hệ thống ParkSmart.</p>
                                <p>Mã nhân viên: <strong>{employeeCode}</strong></p>
                                <p>Vui lòng bấm nút bên dưới để nhập số điện thoại và đặt mật khẩu.</p>
                                <p style='margin: 26px 0;'>
                                    <a href='{inviteUrl}' style='display: inline-block; background: #2563eb; color: #ffffff; text-decoration: none; padding: 12px 18px; border-radius: 10px; font-weight: 700;'>Hoàn tất tài khoản</a>
                                </p>
                                <p style='font-size: 13px; color: #64748b;'>Link có hiệu lực đến {inviteExpiry.ToLocalTime():dd/MM/yyyy HH:mm}.</p>
                            </div>
                        </div>");
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
                    ? "Đã tạo lời mời nhân viên."
                    : emailSent
                        ? "Đã gửi lời mời đến email nhân viên. Nhân viên sẽ tự nhập thông tin còn lại và đặt mật khẩu."
                        : $"Đã tạo lời mời nhưng chưa gửi được email. Chi tiết: {emailError}",
                EmployeeCode = employeeCode,
                InviteToken = inviteToken,
                InviteExpiry = inviteExpiry
            };
        }

        public async Task<UpdateEmployeeResultDto> UpdateEmployeeAsync(string employeeId, UpdateEmployeeByManagerDto request)
        {
            try
            {
                var employee = (await _repo.GetAllAsync(includeDeleted: true))
                    .FirstOrDefault(e => e.EmployeeId == employeeId);
                if (employee == null)
                    return new UpdateEmployeeResultDto { Success = false, Message = "Nhân viên không tồn tại" };

                if (!string.IsNullOrWhiteSpace(request.FullName))
                    employee.FullName = request.FullName.Trim();

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    employee.PhoneNumber = request.PhoneNumber.Trim();

                var shiftChanged = false;
                string? normalizedShift = null;
                TimeSpan shiftStart = default;
                TimeSpan shiftEnd = default;
                if (!string.IsNullOrWhiteSpace(request.Shift))
                {
                    if (!ShiftConstants.TryGetShiftWindow(request.Shift, out normalizedShift, out shiftStart, out shiftEnd))
                    {
                        return new UpdateEmployeeResultDto { Success = false, Message = "Ca làm không hợp lệ. Chọn: Sáng, Chiều, Tối" };
                    }

                    shiftChanged = !string.Equals(employee.Shift, normalizedShift, StringComparison.OrdinalIgnoreCase);
                    employee.Shift = normalizedShift;
                }

                if (!string.IsNullOrEmpty(request.Status))
                {
                    if (IsDeletedStatus(request.Status))
                    {
                        employee.IsDeleted = true;
                        if (employee.Account != null) employee.Account.IsActive = false;
                    }
                    else if (IsDisabledStatus(request.Status))
                    {
                        employee.IsDeleted = false;
                        if (employee.Account != null) employee.Account.IsActive = false;
                    }
                    else if (IsActiveStatus(request.Status))
                    {
                        employee.IsDeleted = false;
                        if (employee.Account != null) employee.Account.IsActive = true;
                    }
                }

                await _repo.UpdateAsync(employee);

                var syncedSchedules = shiftChanged && normalizedShift != null
                    ? await SyncUpcomingScheduledShiftsAsync(employee.EmployeeId, normalizedShift, shiftStart, shiftEnd)
                    : 0;

                var syncMessage = syncedSchedules > 0
                    ? $" Đã đồng bộ {syncedSchedules} ca hôm nay/tương lai chưa bắt đầu."
                    : "";
                return new UpdateEmployeeResultDto { Success = true, Message = $"Cập nhật thông tin nhân viên thành công.{syncMessage}" };
            }
            catch (Exception ex)
            {
                return new UpdateEmployeeResultDto { Success = false, Message = $"Lỗi cập nhật nhân viên: {ex.Message}" };
            }
        }

        private async Task<int> SyncUpcomingScheduledShiftsAsync(
            string employeeId,
            string shiftType,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            var today = DateTime.Today;
            var schedules = await _db.ShiftSchedules
                .Where(s => s.EmployeeId == employeeId &&
                            s.WorkDate >= today &&
                            s.Status == ShiftConstants.ScheduledStatus)
                .ToListAsync();

            foreach (var schedule in schedules)
            {
                schedule.ShiftType = shiftType;
                schedule.StartTime = startTime;
                schedule.EndTime = endTime;
                schedule.Note = string.IsNullOrWhiteSpace(schedule.Note)
                    ? "Đồng bộ từ ca mặc định của nhân viên"
                    : schedule.Note;
            }

            if (schedules.Count > 0)
            {
                await _db.SaveChangesAsync();
            }

            return schedules.Count;
        }

        public async Task<DeleteEmployeeResultDto> DeleteEmployeeAsync(DeleteEmployeeDto request)
        {
            try
            {
                var employee = (await _repo.GetAllAsync(includeDeleted: true))
                    .FirstOrDefault(e => e.EmployeeId == request.EmployeeId);
                if (employee == null)
                    return new DeleteEmployeeResultDto { Success = false, Message = "Nhân viên không tồn tại" };

                employee.IsDeleted = true;
                if (employee.Account != null) employee.Account.IsActive = false;

                await _repo.UpdateAsync(employee);

                return new DeleteEmployeeResultDto
                {
                    Success = true,
                    Message = "Đã xóa nhân viên thành công",
                    EmployeeId = request.EmployeeId,
                    NewStatus = DeletedStatus
                };
            }
            catch (Exception ex)
            {
                return new DeleteEmployeeResultDto { Success = false, Message = $"Lỗi xóa nhân viên: {ex.Message}", EmployeeId = request.EmployeeId };
            }
        }

        private static bool IsDeletedStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            var value = status.Trim().ToLowerInvariant();
            return value.Contains("xóa") ||
                   value.Contains("xoa") ||
                   value.Contains("deleted");
        }

        private static bool IsDisabledStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            var value = status.Trim().ToLowerInvariant();
            return value.Contains("vô") ||
                   value.Contains("vo") ||
                   value.Contains("disabled");
        }

        private static bool IsActiveStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            var value = status.Trim().ToLowerInvariant();
            return value.Contains("hoạt") ||
                   value.Contains("hoat") ||
                   value.Contains("active");
        }

        private static string GetManagerStatus(Employee employee)
        {
            if (employee.IsDeleted)
                return DeletedStatus;

            return employee.Account?.IsActive == true ? ActiveStatus : DisabledStatus;
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
            var inviteToken = Guid.NewGuid().ToString("N");

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
                if (!string.IsNullOrWhiteSpace(request.PhoneNumber) ||
                    !string.IsNullOrWhiteSpace(request.Password) ||
                    !string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    return await ConfirmInviteWithEmployeeDetailsAsync(request);
                }

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

        private async Task<ConfirmEmployeeInviteResultDto> ConfirmInviteWithEmployeeDetailsAsync(ConfirmEmployeeInviteDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.InviteToken))
                    return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Thiếu token xác nhận." };

                if (string.IsNullOrWhiteSpace(request.PhoneNumber) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.ConfirmPassword))
                {
                    return new ConfirmEmployeeInviteResultDto
                    {
                        Success = false,
                        Message = "Vui lòng nhập số điện thoại và mật khẩu."
                    };
                }

                var phoneValidation = ValidatePhoneNumber(request.PhoneNumber);
                if (!phoneValidation.IsValid)
                    return new ConfirmEmployeeInviteResultDto { Success = false, Message = phoneValidation.ErrorMessage };

                var passwordValidation = ValidateStrongPassword(request.Password);
                if (!passwordValidation.IsValid)
                    return new ConfirmEmployeeInviteResultDto { Success = false, Message = passwordValidation.ErrorMessage };

                if (request.Password != request.ConfirmPassword)
                    return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Mật khẩu xác nhận không khớp." };

                var invite = await _inviteRepo.GetByTokenAsync(request.InviteToken);
                if (invite == null) return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Link invite không hợp lệ." };
                if (invite.IsUsed) return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Link invite này đã được sử dụng." };
                if (DateTime.UtcNow > invite.ExpiryTime) return new ConfirmEmployeeInviteResultDto { Success = false, Message = "Link invite đã hết hạn." };

                var account = await _accountRepo.GetByEmailAsync(invite.Email);
                if (account != null && !string.Equals(account.Role, "Employee", StringComparison.OrdinalIgnoreCase))
                {
                    return new ConfirmEmployeeInviteResultDto
                    {
                        Success = false,
                        Message = "Email này đã thuộc về một tài khoản khác trong hệ thống."
                    };
                }

                if (account == null)
                {
                    account = new Account
                    {
                        AccountId = $"ACC{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                        Email = invite.Email.Trim().ToLowerInvariant(),
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12),
                        Role = "Employee",
                        IsActive = true,
                        RequirePasswordChange = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _accountRepo.AddAsync(account);
                }
                else
                {
                    var activeEmployee = (await _repo.GetAllAsync(includeDeleted: true))
                        .FirstOrDefault(e => e.AccountId == account.AccountId && !e.IsDeleted);

                    if (account.IsActive && activeEmployee != null)
                    {
                        return new ConfirmEmployeeInviteResultDto
                        {
                            Success = false,
                            Message = "Tài khoản nhân viên này đã được kích hoạt."
                        };
                    }

                    account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);
                    account.IsActive = true;
                    account.RequirePasswordChange = false;
                    await _accountRepo.UpdateAsync(account);
                }

                var employee = (await _repo.GetAllAsync(includeDeleted: true))
                    .FirstOrDefault(e => e.AccountId == account.AccountId);

                if (employee == null)
                {
                    employee = new Employee
                    {
                        EmployeeId = invite.EmployeeCode,
                        EmployeeCode = invite.EmployeeCode,
                        AccountId = account.AccountId,
                        FullName = invite.FullName.Trim(),
                        PhoneNumber = request.PhoneNumber.Trim(),
                        Shift = invite.Shift?.Trim(),
                        IsDeleted = false
                    };
                    await _repo.AddAsync(employee);
                }
                else
                {
                    employee.FullName = invite.FullName.Trim();
                    employee.PhoneNumber = request.PhoneNumber.Trim();
                    employee.Shift = invite.Shift?.Trim();
                    employee.IsDeleted = false;
                    await _repo.UpdateAsync(employee);
                }

                invite.PhoneNumber = request.PhoneNumber.Trim();
                invite.IsUsed = true;
                await _inviteRepo.UpdateAsync(invite);

                return new ConfirmEmployeeInviteResultDto
                {
                    Success = true,
                    Message = "Hoàn tất tài khoản nhân viên thành công. Bạn có thể đăng nhập ngay.",
                    EmployeeCode = employee.EmployeeCode,
                    EmployeeId = employee.EmployeeId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ConfirmInviteWithEmployeeDetailsAsync failed.");
                return new ConfirmEmployeeInviteResultDto { Success = false, Message = $"Lỗi xác nhận invite: {ex.Message}" };
            }
        }

        private string BuildEmployeeInviteUrl(string inviteToken)
        {
            var frontendBaseUrl = _configuration["FrontendBaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                frontendBaseUrl = "https://localhost:63501";
            }

            return $"{frontendBaseUrl}/Auth/EmployeeInvite?token={Uri.EscapeDataString(inviteToken)}";
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
