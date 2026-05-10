using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Interfaces;

namespace ParkingManagement.Web.Controllers.Api
{
    /// <summary>
    /// Authentication API
    /// Handles login, registration, and password management
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class ApiAuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IJwtTokenProvider _tokenProvider;
        private readonly IAccountRepository _accountRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<ApiAuthController> _logger;

        public ApiAuthController(
            IAuthService authService,
            IJwtTokenProvider tokenProvider,
            IAccountRepository accountRepository,
            ICustomerRepository customerRepository,
            ILogger<ApiAuthController> logger)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _accountRepository = accountRepository;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        /// <param name="dto">Email and password</param>
        /// <returns>Login response with JWT token</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse { Message = "Invalid input", Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var result = await _authService.LoginAsync(dto);

            if (!result.Success)
            {
                _logger.LogWarning($"Login failed for {dto.Email}");
                return Unauthorized(new ErrorResponse { Message = result.Message });
            }

            var token = _tokenProvider.GenerateToken(
                result.AccountId ?? "",
                result.Role ?? "",
                result.Email ?? "",
                result.FullName ?? "",
                result.RelatedId  // Pass customerId/employeeId
            );

            var response = new LoginResponseDto
            {
                AccountId = result.AccountId ?? "",
                Role = result.Role ?? "",
                Email = result.Email ?? "",
                FullName = result.FullName ?? "",
                RelatedId = result.RelatedId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            _logger.LogInformation($"Login successful for {dto.Email}");
            return Ok(response);
        }

        /// <summary>
        /// Register new customer account
        /// </summary>
        /// <param name="dto">Registration details</param>
        /// <returns>Registration confirmation</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse { Message = "Invalid input" });

            var result = await _authService.RegisterAsync(dto);

            if (!result.Success)
            {
                _logger.LogWarning($"Registration failed: {result.Message}");
                return BadRequest(new ErrorResponse { Message = result.Message });
            }

            var response = new RegisterResponseDto
            {
                Success = true,
                Message = result.Message,
                Email = result.Data,
                RequiresOtp = true
            };

            _logger.LogInformation($"Customer registration OTP sent: {dto.Email}");
            return Ok(response);
        }

        /// <summary>
        /// Verify customer registration OTP and activate account
        /// </summary>
        /// <param name="dto">Email and OTP code</param>
        /// <returns>Activation confirmation</returns>
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(VerifyOtpResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse { Message = "Invalid input" });

            var result = await _authService.VerifyOtpAsync(dto);

            if (!result.Success)
            {
                _logger.LogWarning($"OTP verification failed for {dto.Email}: {result.Message}");
                return BadRequest(new ErrorResponse { Message = result.Message });
            }

            var response = new VerifyOtpResponseDto
            {
                Success = true,
                Message = result.Message,
                CustomerId = result.Data
            };

            _logger.LogInformation($"Customer registration verified: {dto.Email}");
            return Ok(response);
        }

        /// <summary>
        /// Change password (requires authentication)
        /// </summary>
        /// <param name="dto">Old and new password</param>
        /// <returns>Change password confirmation</returns>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse { Message = "Invalid input" });

            var accountId = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
            {
                _logger.LogWarning("ChangePassword called without valid token");
                return Unauthorized(new ErrorResponse { Message = "Unauthorized" });
            }

            var result = await _authService.ChangePasswordAsync(accountId, dto);

            if (!result.Success)
            {
                _logger.LogWarning($"Change password failed for {accountId}");
                return BadRequest(new ErrorResponse { Message = result.Message });
            }

            var response = new ChangePasswordResponseDto
            {
                Success = true,
                Message = result.Message
            };

            _logger.LogInformation($"Password changed for {accountId}");
            return Ok(response);
        }

        /// <summary>
        /// Get current user profile (requires authentication)
        /// </summary>
        /// <returns>Current user info</returns>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(CurrentUserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            var accountId = User.FindFirst("accountId")?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var fullName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(accountId))
                return Unauthorized(new ErrorResponse { Message = "Invalid token" });

            var response = new CurrentUserDto
            {
                AccountId = accountId,
                Role = role ?? "",
                Email = email ?? "",
                FullName = fullName ?? ""
            };

            return Ok(response);
        }

        // ═══════════════════════════════════════════════════════════
        // ACCOUNT DELETION APIs
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Customer tự xóa tài khoản của mình (Soft Delete)
        /// </summary>
        /// <returns>Confirmation message</returns>
        [HttpDelete("account")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteMyAccount()
        {
            try
            {
                var accountId = User.FindFirst("accountId")?.Value;
                var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(accountId))
                    return Unauthorized(new { message = "Invalid token" });

                if (role != "Customer")
                    return BadRequest(new { message = "Chỉ khách hàng mới có thể tự xóa tài khoản" });

                // Soft delete customer
                var customer = await _customerRepository.GetByAccountIdAsync(accountId);
                if (customer != null)
                {
                    await _customerRepository.SoftDeleteAsync(customer.CustomerId);
                }

                // Deactivate account
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account != null)
                {
                    account.IsActive = false;
                    await _accountRepository.UpdateAsync(account);
                }

                _logger.LogInformation($"Customer deleted own account: {accountId}");
                return Ok(new { success = true, message = "Tài khoản đã được xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"DeleteMyAccount error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Admin xóa tài khoản bất kỳ (Hard Delete - Xóa vĩnh viễn)
        /// </summary>
        /// <param name="accountId">ID của tài khoản cần xóa</param>
        /// <returns>Confirmation message</returns>
        [HttpDelete("admin/account/{accountId}")]
        [Authorize(Roles = "Manager,Employee")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AdminDeleteAccount(string accountId)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                    return NotFound(new { message = "Không tìm thấy tài khoản" });

                // Không cho xóa Manager
                if (account.Role == "Manager")
                    return BadRequest(new { message = "Không thể xóa tài khoản Manager" });

                // Soft delete customer nếu có
                if (account.Role == "Customer")
                {
                    var customer = await _customerRepository.GetByAccountIdAsync(accountId);
                    if (customer != null)
                    {
                        await _customerRepository.SoftDeleteAsync(customer.CustomerId);
                    }
                }

                // Deactivate account
                account.IsActive = false;
                await _accountRepository.UpdateAsync(account);

                _logger.LogInformation($"Admin deleted account: {accountId}");
                return Ok(new { success = true, message = $"Đã xóa tài khoản {accountId} thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"AdminDeleteAccount error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Admin xóa customer theo CustomerId (Soft Delete)
        /// </summary>
        /// <param name="customerId">ID của customer cần xóa</param>
        /// <returns>Confirmation message</returns>
        [HttpDelete("admin/customer/{customerId}")]
        [Authorize(Roles = "Manager,Employee")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AdminDeleteCustomer(string customerId)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer == null)
                    return NotFound(new { message = "Không tìm thấy khách hàng" });

                // Soft delete customer
                await _customerRepository.SoftDeleteAsync(customerId);

                // Deactivate account
                var account = await _accountRepository.GetByIdAsync(customer.AccountId);
                if (account != null)
                {
                    account.IsActive = false;
                    await _accountRepository.UpdateAsync(account);
                }

                _logger.LogInformation($"Admin deleted customer: {customerId}");
                return Ok(new { success = true, message = $"Đã xóa khách hàng {customer.FullName} thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"AdminDeleteCustomer error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Admin khôi phục tài khoản đã xóa
        /// </summary>
        /// <param name="accountId">ID của tài khoản cần khôi phục</param>
        /// <returns>Confirmation message</returns>
        [HttpPatch("admin/account/{accountId}/restore")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreAccount(string accountId)
        {
            try
            {
                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                    return NotFound(new { message = "Không tìm thấy tài khoản" });

                // Kích hoạt lại account
                account.IsActive = true;
                await _accountRepository.UpdateAsync(account);

                // Restore customer nếu có
                if (account.Role == "Customer")
                {
                    var customer = await _customerRepository.GetByAccountIdAsync(accountId);
                    if (customer != null)
                    {
                        await _customerRepository.RestoreAsync(customer.CustomerId);
                    }
                }

                _logger.LogInformation($"Admin restored account: {accountId}");
                return Ok(new { success = true, message = $"Đã khôi phục tài khoản {accountId} thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"RestoreAccount error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }

        /// <summary>
        /// Lấy danh sách tài khoản đã bị xóa
        /// </summary>
        /// <returns>List of deleted accounts</returns>
        [HttpGet("admin/deleted-accounts")]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDeletedAccounts()
        {
            try
            {
                var deletedCustomers = await _customerRepository.GetDeletedAsync();

                var result = deletedCustomers.Select(c => new
                {
                    c.CustomerId,
                    c.FullName,
                    c.AccountId,
                    c.PhoneNumber
                });

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetDeletedAccounts error: {ex.Message}");
                return StatusCode(500, new { message = "Lỗi server" });
            }
        }
    }

    // Response Models
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string>? Errors { get; set; }
    }

    public class CurrentUserDto
    {
        public string AccountId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}