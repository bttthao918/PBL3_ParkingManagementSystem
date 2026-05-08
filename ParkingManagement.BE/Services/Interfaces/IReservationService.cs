using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    public interface IReservationService
    {
        Task<List<ReservationDto>> GetAllAsync();
        Task<List<ReservationDto>> GetByCustomerIdAsync(string customerId);
        Task<ServiceResult<ReservationDto>> CreateAsync(CreateReservationDto dto);

        /// <summary>
        /// UC005.1 - Lấy danh sách đặt chỗ của khách hàng với phân trang và filter
        /// Tự động cập nhật status thành "Hết hạn" nếu quá giờ hẹn
        /// </summary>
        Task<ListReservationDto> GetByCustomerIdPaginatedAsync(string customerId, FilterReservationDto filter);

        /// <summary>
        /// UC005.2 - Hủy đơn đặt chỗ
        /// Chỉ có thể hủy nếu Status = "Chờ" và chưa quá giờ hẹn
        /// </summary>
        Task<ServiceResult<CancelReservationResultDto>> CancelReservationAsync(string customerId, string reservationId);

        [Obsolete("Use CancelReservationAsync instead")]
        Task<ServiceResult<string>> CancelAsync(string reservationId);
    }
}
