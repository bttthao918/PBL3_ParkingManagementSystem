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
                    relatedId = account.AccountId;  // Manager doesn't need separate ID
                    fullName = "Manager";  // TODO: Get from Account navigation if available
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
                await _emailService.SendOtpEmailAsync(email, dto.FullName.Trim(), otpCode);

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
                _logger.LogError($"RegisterAsync error: {ex.Message}");
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
