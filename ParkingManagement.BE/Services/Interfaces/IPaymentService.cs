using ParkingManagement.BLL.DTOs;

namespace ParkingManagement.BLL.Services.Interfaces
{
    /// <summary>
    /// Payment Service Interface
    /// Xử lý logic thanh toán cho vé lượt & vé tháng
    /// 
    /// Các phương thức thanh toán:
    /// 1. Tiền mặt (Cash)
    /// 2. Chuyển khoản (Bank Transfer)
    /// 3. Ví điện tử (E-Wallet: Momo, ZaloPay)
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Lấy thông tin phương thức thanh toán
        /// - Tiền mặt: số tiền
        /// - Chuyển khoản: thông tin ngân hàng + QR Code
        /// - Ví điện tử: Momo/ZaloPay + QR Code + link thanh toán
        /// </summary>
        Task<PaymentMethodInfoDto> GetPaymentMethodInfoAsync(string paymentMethod, decimal amount);

        /// <summary>
        /// Xác nhận thanh toán
        /// - Validate phương thức thanh toán
        /// - Tạo bản ghi Payment
        /// - Cập nhật trạng thái vé
        /// </summary>
        Task<ServiceResult<PaymentCompletedDto>> ConfirmPaymentAsync(ConfirmPaymentDto confirmDto);

        /// <summary>
        /// Lấy chi tiết một thanh toán
        /// </summary>
        Task<PaymentCompletedDto?> GetPaymentByIdAsync(string paymentId);

        /// <summary>
        /// Lấy danh sách thanh toán của một vé
        /// </summary>
        Task<List<PaymentCompletedDto>> GetPaymentsByTicketAsync(string ticketId);

        /// <summary>
        /// Lấy danh sách thanh toán của vé tháng
        /// </summary>
        Task<List<PaymentCompletedDto>> GetPaymentsByMonthlyTicketAsync(string monthlyTicketId);

        /// <summary>
        /// Lấy tất cả thanh toán (cho thống kê)
        /// </summary>
        Task<List<PaymentCompletedDto>> GetAllPaymentsAsync();

        /// <summary>
        /// Lấy thanh toán trong khoảng thời gian (cho thống kê)
        /// </summary>
        Task<List<PaymentCompletedDto>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to);

        /// <summary>
        /// Tạo QR Code cho chuyển khoản ngân hàng
        /// </summary>
        Task<string> GenerateQRCodeForBankTransferAsync(decimal amount);

        /// <summary>
        /// Tạo QR Code cho ví điện tử
        /// </summary>
        Task<string> GenerateQRCodeForEWalletAsync(string walletType, decimal amount);

        /// <summary>
        /// Tạo link thanh toán cho ví điện tử
        /// </summary>
        Task<string> GeneratePaymentLinkAsync(string walletType, decimal amount);
    }
}
