using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult> LoginAsync(LoginDto dto);
        Task<ServiceResult<string>> RegisterAsync(RegisterDto dto);
        Task<ServiceResult<string>> VerifyOtpAsync(VerifyOtpDto dto);
        Task<ServiceResult> ChangePasswordAsync(string accountId, ChangePasswordDto dto);
        Task<ServiceResult<CurrentUserProfileDto>> GetCurrentProfileAsync(string accountId);
        Task<ServiceResult<CurrentUserProfileDto>> UpdateCurrentProfileAsync(string accountId, UpdateCurrentProfileDto dto);
    }
}
