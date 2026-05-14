namespace ParkingManagement.BLL.DTOs
{
    // ── LOGIN ────────────────────────────────────────
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string AccountId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? RelatedId { get; set; } // CustomerId, EmployeeId, or ManagerId
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    // ── REGISTER ────────────────────────────────────────
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class RegisterResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? CustomerId { get; set; }
        public string? Email { get; set; }
        public bool RequiresOtp { get; set; }
    }

    public class VerifyOtpDto
    {
        public string Email { get; set; } = string.Empty;
        public string Otp { get; set; } = string.Empty;
    }

    public class VerifyOtpResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? CustomerId { get; set; }
    }

    // ── CHANGE PASSWORD ────────────────────────────────────────
    public class ChangePasswordDto
    {
        public string OldPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    // ── CURRENT PROFILE ────────────────────────────────────────
    public class CurrentUserProfileDto
    {
        public string AccountId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? RelatedId { get; set; }
    }

    public class UpdateCurrentProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
    }

    public class UpdateCurrentProfileResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CurrentUserProfileDto? Data { get; set; }
    }

    // ── REFRESH TOKEN ────────────────────────────────────────
    public class RefreshTokenDto
    {
        public string Token { get; set; } = string.Empty;
    }

    // ── GENERIC SERVICE RESULT ────────────────────────────────────────
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ServiceResult<T> Ok(T data, string? msg = null) =>
            new() { Success = true, Data = data, Message = msg ?? "Success" };

        public static ServiceResult<T> Fail(string msg) =>
            new() { Success = false, Message = msg };
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? AccountId { get; set; }
        public string? Role { get; set; }
        public string? FullName { get; set; }
        public string? RelatedId { get; set; }
        public string? Email { get; set; }

        public static ServiceResult CreateSuccess(string accountId, string role, string fullName, string relatedId, string email, string message = "Success")
        {
            return new ServiceResult
            {
                Success = true,
                Message = message,
                AccountId = accountId,
                Role = role,
                FullName = fullName,
                RelatedId = relatedId,
                Email = email
            };
        }

        public static ServiceResult Fail(string message)
        {
            return new ServiceResult { Success = false, Message = message };
        }
    }
}
