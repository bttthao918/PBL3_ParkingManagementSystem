namespace ParkingManagement.BLL.Constants
{
    /// <summary>
    /// Danh sách các phương thức thanh toán
    /// </summary>
    public static class PaymentMethods
    {
        public const string CASH = "Tiền mặt";                    // Cash / Tiền mặt
        public const string BANK_TRANSFER = "Chuyển khoản";       // Bank Transfer / Chuyển khoản ngân hàng
        public const string E_WALLET = "Ví điện tử";              // E-Wallet / Ví điện tử (Momo, ZaloPay, etc.)

        public static List<string> GetAll() => new()
        {
            CASH,
            BANK_TRANSFER,
            E_WALLET
        };
    }

    /// <summary>
    /// Trạng thái thanh toán
    /// </summary>
    public static class PaymentStatuses
    {
        public const string PENDING = "Chờ thanh toán";           // Pending
        public const string COMPLETED = "Hoàn tất";               // Completed
        public const string FAILED = "Thất bại";                  // Failed
        public const string CANCELLED = "Hủy";                    // Cancelled
    }

    /// <summary>
    /// Thông tin ngân hàng cho chuyển khoản
    /// </summary>
    public static class BankInfo
    {
        public const string BANK_NAME = "Vietcombank";            // Tên ngân hàng
        public const string BANK_ACCOUNT = "1234567890";          // Số tài khoản
        public const string ACCOUNT_HOLDER = "Bãi Xe Thông Minh";  // Chủ tài khoản
    }

    /// <summary>
    /// Thông tin ví điện tử
    /// </summary>
    public static class EWalletInfo
    {
        public const string MOMO_PHONE = "0388888888";            // Momo
        public const string ZALOPAY_PHONE = "0377777777";         // ZaloPay
    }
}
