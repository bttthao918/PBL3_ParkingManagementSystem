using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services;
using ParkingManagement.BLL.Services.Interfaces;

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

        public ApiAuthController(
            IAuthService authService,
            IJwtTokenProvider tokenProvider,
            ILogger<ApiAuthController> logger)
        {
            _authService = authService;
            _tokenProvider = tokenProvider;
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
        [ProducesResponseType(typeof(CurrentUserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var accountId = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized(new ErrorResponse { Message = "Invalid token" });

            var result = await _authService.GetCurrentProfileAsync(accountId);
            if (!result.Success)
                return BadRequest(new ErrorResponse { Message = result.Message });

            return Ok(result.Data);
        }

        /// <summary>
        /// Update current user profile. Email is managed by account identity and cannot be changed here.
        /// </summary>
        /// <param name="dto">Profile fields that the current user can edit</param>
        /// <returns>Updated current user profile</returns>
        [HttpPut("me")]
        [Authorize]
        [ProducesResponseType(typeof(UpdateCurrentProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateCurrentProfileDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse { Message = "Invalid input" });

            var accountId = User.FindFirst("accountId")?.Value;
            if (string.IsNullOrEmpty(accountId))
                return Unauthorized(new ErrorResponse { Message = "Invalid token" });

            var result = await _authService.UpdateCurrentProfileAsync(accountId, dto);
            if (!result.Success)
                return BadRequest(new ErrorResponse { Message = result.Message });

            var response = new UpdateCurrentProfileResponseDto
            {
                Success = true,
                Message = result.Message,
                Data = result.Data
            };

            return Ok(response);
        }
    }

    // Response Models
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string>? Errors { get; set; }
    }

}
