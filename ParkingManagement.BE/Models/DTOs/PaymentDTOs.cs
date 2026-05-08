namespace ParkingManagement.BLL.DTOs
{
    // ════════════════════════════════════════════════════════════
    // Payment - Thanh Toán
    // ════════════════════════════════════════════════════════════

    /// <summary>
    /// DTO cho chọn phương thức thanh toán
    /// </summary>
    public class PaymentMethodSelectionDto
    {
        public string PaymentMethod { get; set; } = null!;  // Tiền mặt / Chuyển khoản / Ví điện tử
    }

    /// <summary>
    /// DTO cho thông tin thanh toán tiền mặt
    /// </summary>
    public class CashPaymentDto
    {
        public decimal Amount { get; set; }
        public decimal ReceivedAmount { get; set; }  // Số tiền khách đưa
        public decimal Change { get; set; }          // Tiền thừa
    }

    /// <summary>
    /// DTO cho thông tin thanh toán chuyển khoản
    /// </summary>
    public class BankTransferPaymentDto
    {
        public string BankName { get; set; } = null!;           // Tên ngân hàng (Vietcombank)
        public string BankAccount { get; set; } = null!;        // Số tài khoản
        public string AccountHolder { get; set; } = null!;      // Chủ tài khoản
        public decimal Amount { get; set; }                     // Số tiền cần chuyển
        public string TransferContent { get; set; } = null!;   // Nội dung chuyển khoản
        public string QRCodeBase64 { get; set; } = null!;       // QR Code (Base64)
        public string QRCodeUrl { get; set; } = null!;          // URL để khách scan
    }

    /// <summary>
    /// DTO cho thông tin thanh toán ví điện tử
    /// </summary>
    public class EWalletPaymentDto
    {
        public string WalletType { get; set; } = null!;         // Momo / ZaloPay
        public string PhoneNumber { get; set; } = null!;        // Số điện thoại
        public decimal Amount { get; set; }                     // Số tiền cần thanh toán
        public string QRCodeBase64 { get; set; } = null!;       // QR Code (Base64)
        public string PaymentLink { get; set; } = null!;        // Link thanh toán
    }

    /// <summary>
    /// DTO response khi chọn phương thức thanh toán
    /// </summary>
    public class PaymentMethodInfoDto
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; } = null!;      // Phương thức đã chọn
        
        // Tùy theo phương thức, return một trong các DTO dưới:
        public CashPaymentDto? CashPayment { get; set; }
        public BankTransferPaymentDto? BankTransfer { get; set; }
        public EWalletPaymentDto? EWallet { get; set; }
        
        public string? Message { get; set; }
    }

    /// <summary>
    /// DTO cho xác nhận thanh toán (sau khi khách thanh toán)
    /// </summary>
    public class ConfirmPaymentDto
    {
        public string? TicketId { get; set; }                    // Mã vé
        public string? MonthlyTicketId { get; set; }             // Mã vé tháng
        public string PaymentMethod { get; set; } = null!;      // Phương thức thanh toán
        public decimal Amount { get; set; }                     // Số tiền thanh toán

        // Tùy phương thức
        public decimal? ReceivedAmount { get; set; }            // Tiền nhận (tiền mặt)
        public string? BankTransferRef { get; set; }            // Mã tham chiếu chuyển khoản
        public string? EWalletTransactionId { get; set; }       // Mã giao dịch ví điện tử
    }

    /// <summary>
    /// DTO response sau khi thanh toán thành công
    /// </summary>
    public class PaymentCompletedDto
    {
        public bool Success { get; set; }
        public string? PaymentId { get; set; }                  // Mã thanh toán
        public string? TicketId { get; set; }                   // Mã vé
        public string? MonthlyTicketId { get; set; }            // Mã vé tháng
        public string PaymentMethod { get; set; } = null!;      // Phương thức
        public decimal Amount { get; set; }                     // Số tiền
        public DateTime PaymentTime { get; set; }               // Thời gian thanh toán
        public decimal? Change { get; set; }                    // Tiền thừa (nếu tiền mặt)
        public string? PaymentStatus { get; set; }              // Trạng thái thanh toán
        public string? Message { get; set; }
    }
}
