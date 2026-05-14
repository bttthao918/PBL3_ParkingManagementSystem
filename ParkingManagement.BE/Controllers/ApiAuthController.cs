using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Data;
using System.Security.Claims;

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
        private readonly ILogger<ApiAuthController> _logger;
        private readonly AppDbContext _db;

        public ApiAuthController(
            IAuthService authService,
            IJwtTokenProvider tokenProvider,
            ILogger<ApiAuthController> logger,
            AppDbContext db)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
            _logger = logger;
            _db = db;
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
        [AllowAnonymous]
        [ProducesResponseType(typeof(ChangePasswordResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto, [FromQuery] string? accountId = null)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse { Message = "Invalid input" });

            var resolvedAccountId = ResolveProfileAccountId(accountId);
            if (string.IsNullOrWhiteSpace(resolvedAccountId))
            {
                _logger.LogWarning("ChangePassword called without valid account id");
                return Unauthorized(new ErrorResponse { Message = "Unauthorized" });
            }

            var result = await _authService.ChangePasswordAsync(resolvedAccountId, dto);

            if (!result.Success)
            {
                _logger.LogWarning($"Change password failed for {resolvedAccountId}");
                return BadRequest(new ErrorResponse { Message = result.Message });
            }

            var response = new ChangePasswordResponseDto
            {
                Success = true,
                Message = result.Message
            };

            _logger.LogInformation($"Password changed for {resolvedAccountId}");
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

        [HttpGet("profile")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AccountProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile([FromQuery] string? accountId = null)
        {
            var resolvedAccountId = ResolveProfileAccountId(accountId);
            if (string.IsNullOrWhiteSpace(resolvedAccountId))
                return Unauthorized(new ErrorResponse { Message = "Invalid token" });

            var profile = await BuildProfileAsync(resolvedAccountId);
            if (profile == null)
                return NotFound(new ErrorResponse { Message = "Không tìm thấy tài khoản" });

            return Ok(profile);
        }

        [HttpPut("profile")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AccountProfileUpdateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AccountProfileUpdateResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateAccountProfileDto dto, [FromQuery] string? accountId = null)
        {
            var resolvedAccountId = ResolveProfileAccountId(accountId);
            if (string.IsNullOrWhiteSpace(resolvedAccountId))
                return Unauthorized(new ErrorResponse { Message = "Invalid token" });

            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest(new AccountProfileUpdateResponseDto { Success = false, Message = "Họ và tên không được để trống." });

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest(new AccountProfileUpdateResponseDto { Success = false, Message = "Email không được để trống." });

            var account = await _db.Accounts
                .Include(a => a.Manager)
                .Include(a => a.Employee)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountId == resolvedAccountId);

            if (account == null)
                return BadRequest(new AccountProfileUpdateResponseDto { Success = false, Message = "Không tìm thấy tài khoản." });

            var email = dto.Email.Trim().ToLowerInvariant();
            var emailExists = await _db.Accounts.AnyAsync(a => a.AccountId != resolvedAccountId && a.Email == email);
            if (emailExists)
                return BadRequest(new AccountProfileUpdateResponseDto { Success = false, Message = "Email này đã được sử dụng bởi tài khoản khác." });

            account.Email = email;

            var fullName = dto.FullName.Trim();
            var phoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
            var gender = NormalizeGenderToDatabase(dto.Gender);

            switch (account.Role)
            {
                case "Manager" when account.Manager != null:
                    account.Manager.FullName = fullName;
                    account.Manager.PhoneNumber = phoneNumber;
                    account.Manager.Gender = gender;
                    break;
                case "Employee" when account.Employee != null:
                    account.Employee.FullName = fullName;
                    account.Employee.PhoneNumber = phoneNumber;
                    account.Employee.Gender = gender;
                    break;
                case "Customer" when account.Customer != null:
                    account.Customer.FullName = fullName;
                    account.Customer.PhoneNumber = phoneNumber;
                    account.Customer.Gender = gender;
                    break;
                default:
                    return BadRequest(new AccountProfileUpdateResponseDto { Success = false, Message = "Không tìm thấy hồ sơ tương ứng với vai trò tài khoản." });
            }

            await _db.SaveChangesAsync();

            return Ok(new AccountProfileUpdateResponseDto
            {
                Success = true,
                Message = "Cập nhật thông tin thành công.",
                Data = await BuildProfileAsync(resolvedAccountId)
            });
        }

        private string? ResolveProfileAccountId(string? fallbackAccountId)
        {
            var tokenAccountId = User.FindFirst("accountId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrWhiteSpace(tokenAccountId))
                return tokenAccountId;

            return string.IsNullOrWhiteSpace(fallbackAccountId)
                ? null
                : fallbackAccountId.Trim();
        }

        private async Task<AccountProfileDto?> BuildProfileAsync(string accountId)
        {
            var account = await _db.Accounts
                .Include(a => a.Manager)
                .Include(a => a.Employee)
                .Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
                return null;

            var fullName = account.Role switch
            {
                "Manager" => account.Manager?.FullName,
                "Employee" => account.Employee?.FullName,
                "Customer" => account.Customer?.FullName,
                _ => null
            };

            var phoneNumber = account.Role switch
            {
                "Manager" => account.Manager?.PhoneNumber,
                "Employee" => account.Employee?.PhoneNumber,
                "Customer" => account.Customer?.PhoneNumber,
                _ => null
            };

            var gender = account.Role switch
            {
                "Manager" => account.Manager?.Gender,
                "Employee" => account.Employee?.Gender,
                "Customer" => account.Customer?.Gender,
                _ => null
            };

            return new AccountProfileDto
            {
                AccountId = account.AccountId,
                FullName = fullName ?? "",
                Email = account.Email,
                PhoneNumber = phoneNumber ?? "",
                RoleName = account.Role,
                Gender = NormalizeGenderToDisplay(gender),
                CreatedAt = account.CreatedAt
            };
        }

        private static string NormalizeGenderToDisplay(string? gender)
        {
            return gender switch
            {
                "Male" => "Nam",
                "Female" => "Nữ",
                "Other" => "Khác",
                "Nam" => "Nam",
                "Nữ" => "Nữ",
                "Khác" => "Khác",
                _ => "Khác"
            };
        }

        private static string? NormalizeGenderToDatabase(string? gender)
        {
            return gender switch
            {
                "Nam" => "Male",
                "Nữ" => "Female",
                "Khác" => "Other",
                "Male" => "Male",
                "Female" => "Female",
                "Other" => "Other",
                _ => null
            };
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

    public class AccountProfileDto
    {
        public string AccountId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateAccountProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
    }

    public class AccountProfileUpdateResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AccountProfileDto? Data { get; set; }
    }
}
