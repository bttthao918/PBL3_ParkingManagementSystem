using BCrypt.Net;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Interfaces;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Security.Cryptography;

namespace ParkingManagement.BLL.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAccountRepository _accountRepo;
        private readonly ICustomerRepository _customerRepo;
        private readonly IEmployeeRepository _employeeRepo;
        private readonly IManagerRepository _managerRepo;
        private readonly IOtpRepository _otpRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IAccountRepository accountRepo,
            ICustomerRepository customerRepo,
            IEmployeeRepository employeeRepo,
            IManagerRepository managerRepo,
            IOtpRepository otpRepo,
            IEmailService emailService,
            ILogger<AuthService> logger)
        {
            _accountRepo = accountRepo;
            _customerRepo = customerRepo;
            _employeeRepo = employeeRepo;
            _managerRepo = managerRepo;
            _otpRepo = otpRepo;
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        public async Task<ServiceResult> LoginAsync(LoginDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                    return ServiceResult.Fail("Email và mật khẩu không được để trống.");

                var account = await _accountRepo.GetByEmailAsync(dto.Email.Trim().ToLower());
                if (account == null)
                {
                    _logger.LogWarning($"Login failed for email: {dto.Email}");
                    return ServiceResult.Fail("Email hoặc mật khẩu không đúng.");
                }

                if (!account.IsActive)
                {
                    _logger.LogWarning($"Inactive account login attempted for email: {dto.Email}");
                    return ServiceResult.Fail("Tài khoản chưa được xác thực hoặc đã bị khóa.");
                }

                if (!BCrypt.Net.BCrypt.Verify(dto.Password, account.PasswordHash))
                {
                    _logger.LogWarning($"Invalid password for email: {dto.Email}");
                    return ServiceResult.Fail("Email hoặc mật khẩu không đúng.");
                }

                string? relatedId = null;
                string? fullName = null;

                if (account.Role == "Customer")
                {
                    var customer = await _customerRepo.GetByAccountIdAsync(account.AccountId);
                    relatedId = customer?.CustomerId;
                    fullName = customer?.FullName;
                }
                else if (account.Role == "Employee")
                {
                    var employee = await _employeeRepo.GetByAccountIdAsync(account.AccountId);
                    relatedId = employee?.EmployeeId;
                    fullName = employee?.FullName;
                }
                else if (account.Role == "Manager")
                {
                    var manager = await GetManagerByAccountIdAsync(account.AccountId);
                    relatedId = manager?.ManagerId ?? account.AccountId;
                    fullName = manager?.FullName ?? "Manager";
                }

                _logger.LogInformation($"Login successful for {account.Role}: {account.Email}");
                return ServiceResult.CreateSuccess(account.AccountId, account.Role, fullName ?? "", relatedId ?? "", account.Email, "Đăng nhập thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"LoginAsync error: {ex.Message}");
                return ServiceResult.Fail("Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        /// <summary>
        /// Register customer account. Account is inactive until OTP verification succeeds.
        /// </summary>
        public async Task<ServiceResult<string>> RegisterAsync(RegisterDto dto)
        {
            try
            {
                // Validate
                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password) ||
                    string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.PhoneNumber))
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        Message = "Vui lòng nhập đầy đủ các trường bắt buộc."
                    };
                }

                if (dto.Password != dto.ConfirmPassword)
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        Message = "Mật khẩu xác nhận không trùng khớp."
                    };
                }

                if (!IsValidEmail(dto.Email))
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        Message = "Địa chỉ email không hợp lệ."
                    };
                }

                if (!IsStrongPassword(dto.Password))
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        Message = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm: chữ hoa, chữ thường, chữ số và ký tự đặc biệt."
                    };
                }

                var email = dto.Email.Trim().ToLower();
                var existingAccount = await _accountRepo.GetByEmailAsync(email);
                if (existingAccount != null && existingAccount.IsActive)
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        Message = "Email này đã được đăng ký."
                    };
                }

                if (existingAccount != null && existingAccount.Role != "Customer")
                {
                    return new ServiceResult<string>
                    {
                        Success = false,
                        Message = "Email này đang thuộc tài khoản khác trong hệ thống."
                    };
                }

                Account account;
                Customer? customer;

                if (existingAccount == null)
                {
                    var accountId = $"ACC{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
                    account = new Account
                    {
                        AccountId = accountId,
                        Email = email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
                        Role = "Customer",
                        CreatedAt = DateTime.Now,
                        IsActive = false,
                        RequirePasswordChange = false
                    };

                    await _accountRepo.AddAsync(account);

                    customer = new Customer
                    {
                        CustomerId = $"CUS{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                        AccountId = account.AccountId,
                        FullName = dto.FullName.Trim(),
                        PhoneNumber = dto.PhoneNumber.Trim(),
                        IsDeleted = false
                    };

                    await _customerRepo.AddAsync(customer);
                }
                else
                {
                    account = existingAccount;
                    account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12);
                    account.IsActive = false;
                    account.RequirePasswordChange = false;
                    await _accountRepo.UpdateAsync(account);

                    customer = await _customerRepo.GetByAccountIdAsync(account.AccountId);
                    if (customer == null)
                    {
                        customer = new Customer
                        {
                            CustomerId = $"CUS{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}",
                            AccountId = account.AccountId,
                            FullName = dto.FullName.Trim(),
                            PhoneNumber = dto.PhoneNumber.Trim(),
                            IsDeleted = false
                        };

                        await _customerRepo.AddAsync(customer);
                    }
                    else
                    {
                        customer.FullName = dto.FullName.Trim();
                        customer.PhoneNumber = dto.PhoneNumber.Trim();
                        customer.IsDeleted = false;
                        await _customerRepo.UpdateAsync(customer);
                    }
                }

                var otpCode = await CreateRegistrationOtpAsync(email);
                try
                {
                    await _emailService.SendOtpEmailAsync(email, dto.FullName.Trim(), otpCode);
                }
                catch (Exception mailEx)
                {
                    _logger.LogError(mailEx, "Failed to send registration OTP email to {Email}.", email);
                    return ServiceResult<string>.Fail("Không gửi được mã OTP đến email. Vui lòng kiểm tra cấu hình Gmail hoặc thử lại sau.");
                }

                _logger.LogInformation($"Registration OTP sent for customer: {email}");
                return new ServiceResult<string>
                {
                    Success = true,
                    Message = "Mã OTP xác thực đã được gửi đến email của bạn.",
                    Data = email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RegisterAsync error.");
                return new ServiceResult<string>
                {
                    Success = false,
                    Message = "Lỗi hệ thống. Vui lòng thử lại."
                };
            }
        }

        /// <summary>
        /// Verify customer registration OTP and activate the pending account.
        /// </summary>
        public async Task<ServiceResult<string>> VerifyOtpAsync(VerifyOtpDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Otp))
                    return ServiceResult<string>.Fail("Email và OTP không được để trống.");

                var email = dto.Email.Trim().ToLower();
                var code = dto.Otp.Trim();

                var otp = await _otpRepo.GetLatestByEmailAsync(email);
                if (otp == null || otp.Code != code)
                    return ServiceResult<string>.Fail("OTP không hợp lệ hoặc đã hết hạn.");

                var account = await _accountRepo.GetByEmailAsync(email);
                if (account == null || account.Role != "Customer")
                    return ServiceResult<string>.Fail("Không tìm thấy đăng ký chờ xác thực.");

                var customer = await _customerRepo.GetByAccountIdAsync(account.AccountId);
                if (customer == null)
                    return ServiceResult<string>.Fail("Không tìm thấy hồ sơ khách hàng chờ xác thực.");

                account.IsActive = true;
                await _accountRepo.UpdateAsync(account);

                otp.IsVerified = true;
                otp.VerifiedAt = DateTime.UtcNow;
                await _otpRepo.UpdateAsync(otp);

                _logger.LogInformation($"Customer account verified by OTP: {email}");
                return ServiceResult<string>.Ok(customer.CustomerId, "Xác thực email thành công. Tài khoản đã được kích hoạt.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"VerifyOtpAsync error: {ex.Message}");
                return ServiceResult<string>.Fail("Lỗi hệ thống. Vui lòng thử lại.");
            }
        }

        public async Task<ServiceResult> RequestPasswordResetAsync(ForgotPasswordRequestDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email))
                    return ServiceResult.Fail("Vui lòng nhập email.");

                if (!IsValidEmail(dto.Email))
                    return ServiceResult.Fail("Địa chỉ email không hợp lệ.");

                var email = dto.Email.Trim().ToLower();
                var account = await _accountRepo.GetByEmailAsync(email);
                if (account == null || !account.IsActive)
                    return ServiceResult.Fail("Không tìm thấy tài khoản đang hoạt động với email này.");

                var profile = await BuildCurrentProfileAsync(account);
                var otpCode = await CreateRegistrationOtpAsync(email);
                try
                {
                    await _emailService.SendPasswordResetOtpEmailAsync(email, profile?.FullName ?? account.Email, otpCode);
                }
                catch (Exception mailEx)
                {
                    _logger.LogError(mailEx, "Failed to send password reset OTP email to {Email}.", email);
                    return ServiceResult.Fail("Không gửi được mã OTP đến email. Vui lòng kiểm tra cấu hình Gmail hoặc thử lại sau.");
                }

                _logger.LogInformation($"Password reset OTP sent for account: {email}");
                return new ServiceResult
                {
                    Success = true,
                    Message = "Mã OTP đặt lại mật khẩu đã được gửi đến email của bạn.",
                    AccountId = account.AccountId,
                    Role = account.Role,
                    FullName = profile?.FullName,
                    RelatedId = profile?.RelatedId,
                    Email = account.Email
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RequestPasswordResetAsync error.");
                return ServiceResult.Fail("Không thể gửi OTP đặt lại mật khẩu. Vui lòng thử lại.");
            }
        }

        public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Email) ||
                    string.IsNullOrWhiteSpace(dto.Otp) ||
                    string.IsNullOrWhiteSpace(dto.NewPassword) ||
                    string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                {
                    return ServiceResult.Fail("Vui lòng nhập đầy đủ thông tin.");
                }

                if (dto.NewPassword != dto.ConfirmPassword)
                    return ServiceResult.Fail("Mật khẩu xác nhận không trùng khớp.");

                if (!IsStrongPassword(dto.NewPassword))
                    return ServiceResult.Fail("Mật khẩu mới phải có ít nhất 8 ký tự, bao gồm: chữ hoa, chữ thường, chữ số và ký tự đặc biệt.");

                var email = dto.Email.Trim().ToLower();
                var code = dto.Otp.Trim();
                var account = await _accountRepo.GetByEmailAsync(email);
                if (account == null || !account.IsActive)
                    return ServiceResult.Fail("Không tìm thấy tài khoản đang hoạt động.");

                var otp = await _otpRepo.GetLatestByEmailAsync(email);
                if (otp == null || otp.Code != code)
                    return ServiceResult.Fail("OTP không hợp lệ hoặc đã hết hạn.");

                account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
                await _accountRepo.UpdateAsync(account);

                otp.IsVerified = true;
                otp.VerifiedAt = DateTime.UtcNow;
                await _otpRepo.UpdateAsync(otp);

                _logger.LogInformation($"Password reset completed for account: {email}");
                return ServiceResult.CreateSuccess(account.AccountId, account.Role, "", "", account.Email, "Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"ResetPasswordAsync error: {ex.Message}");
                return ServiceResult.Fail("Không thể đặt lại mật khẩu. Vui lòng thử lại.");
            }
        }

        /// <summary>
        /// Change password
        /// </summary>
        public async Task<ServiceResult> ChangePasswordAsync(string accountId, ChangePasswordDto dto)
        {
            try
            {
                var account = await _accountRepo.GetByIdAsync(accountId);
                if (account == null)
                    return ServiceResult.Fail("Tài khoản không tồn tại.");

                if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, account.PasswordHash))
                    return ServiceResult.Fail("Mật khẩu cũ không đúng.");

                if (dto.NewPassword != dto.ConfirmPassword)
                    return ServiceResult.Fail("Mật khẩu xác nhận không trùng khớp.");

                if (!IsStrongPassword(dto.NewPassword))
                    return ServiceResult.Fail("Mật khẩu mới không đủ mạnh.");

                account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 12);
                await _accountRepo.UpdateAsync(account);

                _logger.LogInformation($"Password changed for account: {accountId}");
                return ServiceResult.CreateSuccess(account.AccountId, account.Role, "", "", account.Email, "Đổi mật khẩu thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"ChangePasswordAsync error: {ex.Message}");
                return ServiceResult.Fail("Lỗi hệ thống.");
            }
        }

        public async Task<ServiceResult<CurrentUserProfileDto>> GetCurrentProfileAsync(string accountId)
        {
            try
            {
                var account = await _accountRepo.GetByIdAsync(accountId);
                if (account == null || !account.IsActive)
                    return ServiceResult<CurrentUserProfileDto>.Fail("Tài khoản không tồn tại hoặc đã bị khóa.");

                var profile = await BuildCurrentProfileAsync(account);
                if (profile == null)
                    return ServiceResult<CurrentUserProfileDto>.Fail("Không tìm thấy hồ sơ tài khoản.");

                return ServiceResult<CurrentUserProfileDto>.Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetCurrentProfileAsync error: {ex.Message}");
                return ServiceResult<CurrentUserProfileDto>.Fail("Lỗi hệ thống.");
            }
        }

        public async Task<ServiceResult<CurrentUserProfileDto>> UpdateCurrentProfileAsync(string accountId, UpdateCurrentProfileDto dto)
        {
            try
            {
                var account = await _accountRepo.GetByIdAsync(accountId);
                if (account == null || !account.IsActive)
                    return ServiceResult<CurrentUserProfileDto>.Fail("Tài khoản không tồn tại hoặc đã bị khóa.");

                var fullName = dto.FullName?.Trim() ?? "";
                if (fullName.Length < 3 || fullName.Length > 100)
                    return ServiceResult<CurrentUserProfileDto>.Fail("Họ và tên phải có từ 3 đến 100 ký tự.");

                var phoneNumber = NormalizePhoneNumber(dto.PhoneNumber);
                if (phoneNumber == null)
                    return ServiceResult<CurrentUserProfileDto>.Fail("Số điện thoại không hợp lệ.");

                var gender = NormalizeGender(dto.Gender);
                if (gender == null)
                    return ServiceResult<CurrentUserProfileDto>.Fail("Giới tính không hợp lệ.");

                if (account.Role == "Customer")
                {
                    var customer = await _customerRepo.GetByAccountIdAsync(account.AccountId);
                    if (customer == null)
                        return ServiceResult<CurrentUserProfileDto>.Fail("Không tìm thấy hồ sơ khách hàng.");

                    customer.FullName = fullName;
                    customer.PhoneNumber = phoneNumber;
                    customer.Gender = gender;
                    await _customerRepo.UpdateAsync(customer);
                }
                else if (account.Role == "Employee")
                {
                    var employee = await _employeeRepo.GetByAccountIdAsync(account.AccountId);
                    if (employee == null)
                        return ServiceResult<CurrentUserProfileDto>.Fail("Không tìm thấy hồ sơ nhân viên.");

                    employee.FullName = fullName;
                    employee.PhoneNumber = phoneNumber;
                    employee.Gender = gender;
                    await _employeeRepo.UpdateAsync(employee);
                }
                else if (account.Role == "Manager")
                {
                    var manager = await GetManagerByAccountIdAsync(account.AccountId);
                    if (manager == null)
                        return ServiceResult<CurrentUserProfileDto>.Fail("Không tìm thấy hồ sơ quản lý.");

                    manager.FullName = fullName;
                    manager.PhoneNumber = phoneNumber;
                    manager.Gender = gender;
                    await _managerRepo.UpdateAsync(manager);
                }
                else
                {
                    return ServiceResult<CurrentUserProfileDto>.Fail("Vai trò tài khoản không hợp lệ.");
                }

                var profile = await BuildCurrentProfileAsync(account);
                return profile == null
                    ? ServiceResult<CurrentUserProfileDto>.Fail("Không tìm thấy hồ sơ tài khoản.")
                    : ServiceResult<CurrentUserProfileDto>.Ok(profile, "Cập nhật thông tin thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateCurrentProfileAsync error: {ex.Message}");
                return ServiceResult<CurrentUserProfileDto>.Fail("Lỗi hệ thống.");
            }
        }

        private async Task<CurrentUserProfileDto?> BuildCurrentProfileAsync(Account account)
        {
            if (account.Role == "Customer")
            {
                var customer = await _customerRepo.GetByAccountIdAsync(account.AccountId);
                return customer == null
                    ? null
                    : CreateProfile(account, customer.CustomerId, customer.FullName, customer.PhoneNumber, customer.Gender);
            }

            if (account.Role == "Employee")
            {
                var employee = await _employeeRepo.GetByAccountIdAsync(account.AccountId);
                return employee == null
                    ? null
                    : CreateProfile(account, employee.EmployeeId, employee.FullName, employee.PhoneNumber, employee.Gender);
            }

            if (account.Role == "Manager")
            {
                var manager = await GetManagerByAccountIdAsync(account.AccountId);
                return manager == null
                    ? CreateProfile(account, account.AccountId, "Manager", null, null)
                    : CreateProfile(account, manager.ManagerId, manager.FullName, manager.PhoneNumber, manager.Gender);
            }

            return null;
        }

        private static CurrentUserProfileDto CreateProfile(Account account, string relatedId, string fullName, string? phoneNumber, string? gender)
        {
            return new CurrentUserProfileDto
            {
                AccountId = account.AccountId,
                Role = account.Role,
                Email = account.Email,
                FullName = fullName,
                PhoneNumber = phoneNumber,
                Gender = gender,
                CreatedAt = account.CreatedAt,
                RelatedId = relatedId
            };
        }

        private async Task<Manager?> GetManagerByAccountIdAsync(string accountId)
        {
            var managers = await _managerRepo.GetAllAsync();
            return managers.FirstOrDefault(m => m.AccountId == accountId && !m.IsDeleted);
        }

        private static string? NormalizePhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return null;

            var compact = Regex.Replace(phoneNumber.Trim(), @"[\s\-.]", "");
            return Regex.IsMatch(compact, @"^\+?[0-9]{9,15}$") ? compact : null;
        }

        private static string? NormalizeGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return null;

            return gender.Trim().ToLowerInvariant() switch
            {
                "male" or "nam" => "Male",
                "female" or "nu" or "nữ" => "Female",
                "other" or "khac" or "khác" => "Other",
                _ => null
            };
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

        private bool IsStrongPassword(string password)
        {
            if (password.Length < 8) return false;
            if (!Regex.IsMatch(password, @"[a-z]")) return false;
            if (!Regex.IsMatch(password, @"[A-Z]")) return false;
            if (!Regex.IsMatch(password, @"[0-9]")) return false;
            if (!Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':"",.<>?/\\|`~]")) return false;
            return true;
        }

        private async Task<string> CreateRegistrationOtpAsync(string email)
        {
            await _otpRepo.DeleteExpiredAsync();

            Otp? previousOtp;
            while ((previousOtp = await _otpRepo.GetLatestByEmailAsync(email)) != null)
            {
                previousOtp.IsVerified = true;
                previousOtp.VerifiedAt = DateTime.UtcNow;
                await _otpRepo.UpdateAsync(previousOtp);
            }

            var otpCode = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            var otp = new Otp
            {
                OtpId = await _otpRepo.GenerateIdAsync(),
                Email = email,
                Code = otpCode,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false
            };

            await _otpRepo.AddAsync(otp);
            return otpCode;
        }

        // Original methods for compatibility
        public async Task<LoginResultDto> LoginAsync_Legacy(LoginDto dto)
        {
            var result = await LoginAsync(dto);
            return new LoginResultDto
            {
                Success = result.Success,
                Message = result.Message,
                AccountId = result.AccountId ?? "",
                Role = result.Role ?? "",
                FullName = result.FullName ?? "",
                RelatedId = result.RelatedId
            };
        }

        public async Task<RegisterResultDto> RegisterAsync_Legacy(RegisterDto dto)
        {
            var result = await RegisterAsync(dto);
            return new RegisterResultDto
            {
                Success = result.Success,
                Message = result.Message,
                Email = result.Data
            };
        }
    }

    // Legacy DTOs for backward compatibility
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? RelatedId { get; set; }
    }

    public class RegisterResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? CustomerId { get; set; }
        public string? Email { get; set; }
        public string? Otp { get; set; }
    }
}
