using ParkingManagement.DAL.Models;

namespace ParkingManagement.DAL.Repositories
{
    public interface IPricingRepository
    {
        /// <summary>
        /// Lấy tất cả pricing configurations từ DB
        /// </summary>
        Task<List<PricingConfiguration>> GetAllPricingConfigAsync();

        /// <summary>
        /// Lấy giá vé theo loại xe và loại giá
        /// </summary>
        Task<PricingConfiguration?> GetPricingByTypeAndRateAsync(string vehicleType, string rateType);

        /// <summary>
        /// Thêm hoặc update một giá vé
        /// </summary>
        Task UpsertPricingAsync(PricingConfiguration pricing);

        /// <summary>
        /// Xóa tất cả giá vé cũ (khi update)
        /// </summary>
        Task DeleteAllPricingAsync();
    }
}
