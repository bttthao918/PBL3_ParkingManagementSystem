using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    /// <summary>
    /// UC017 - Quản lý giá vé của Manager
    /// </summary>
    public interface IPricingService
    {
        /// <summary>
        /// UC017.1 - Lấy giá vé hiện tại
        /// </summary>
        Task<PricingDto> GetCurrentPricingAsync();

        /// <summary>
        /// UC017.2 - Cập nhật giá vé
        /// </summary>
        Task<ServiceResult<PricingDto>> UpdatePricingAsync(UpdatePricingDto updateDto, string managerId);
    }
}
