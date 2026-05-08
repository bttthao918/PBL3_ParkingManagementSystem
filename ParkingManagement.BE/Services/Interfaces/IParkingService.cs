using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Interfaces;

public interface IParkingService
{
    /// <summary>
    /// Ghi nhận xe vào bãi.
    /// </summary>
    /// <param name="licensePlate">Biển số xe (không được để trống).</param>
    /// <param name="vehicleType">Loại xe (Motorcycle, Car, …).</param>
    /// <param name="areaId">ID khu bãi đỗ xe.</param>
    /// <returns><see cref="CheckInResult"/> nếu thành công.</returns>
    /// <exception cref="ArgumentException">Biển số để trống.</exception>
    /// <exception cref="InvalidOperationException">Bãi đã đầy hoặc không tìm thấy.</exception>
    Task<CheckInResult> CheckInAsync(string licensePlate, string vehicleType, int areaId);

    /// <summary>
    /// Ghi nhận xe ra khỏi bãi, tính tiền và cập nhật Ticket.
    /// </summary>
    /// <param name="ticketId">ID vé cần checkout.</param>
    /// <returns><see cref="CheckOutResult"/> chứa tổng giờ và giá tiền.</returns>
    /// <exception cref="KeyNotFoundException">Không tìm thấy ticket.</exception>
    /// <exception cref="InvalidOperationException">Ticket không ở trạng thái Active.</exception>
    Task<CheckOutResult> CheckOutAsync(int ticketId);
}
