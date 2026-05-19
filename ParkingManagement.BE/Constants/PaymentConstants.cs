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

        public static readonly string[] SupportedMethods =
        {
            CASH,
            BANK_TRANSFER,
            E_WALLET
        };

        public static List<string> GetAll() => new()
        {
            CASH,
            BANK_TRANSFER,
            E_WALLET
        };

        public static bool IsSupported(string? method) =>
            SupportedMethods.Contains(Normalize(method), StringComparer.OrdinalIgnoreCase);

        public static string Normalize(string? method)
        {
            if (string.IsNullOrWhiteSpace(method))
                return CASH;

            var normalized = method.Trim();

            if (string.Equals(normalized, CASH, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Cash", StringComparison.OrdinalIgnoreCase))
                return CASH;

            if (string.Equals(normalized, BANK_TRANSFER, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "BankTransfer", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Bank Transfer", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "QR Pay", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "QRPay", StringComparison.OrdinalIgnoreCase))
                return BANK_TRANSFER;

            if (string.Equals(normalized, E_WALLET, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "EWallet", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "E-Wallet", StringComparison.OrdinalIgnoreCase))
                return E_WALLET;

            return normalized;
        }
    }

    /// <summary>
    /// Trạng thái thanh toán
    /// </summary>
    public static class PaymentStatuses
    {
        public const string PENDING = "Chờ thanh toán";           // Pending
        public const string SUCCESS = "Thành công";               // Success
        public const string COMPLETED = SUCCESS;                  // Backward-compatible alias
        public const string LEGACY_COMPLETED = "Hoàn tất";        // Legacy value used by older code
        public const string FAILED = "Thất bại";                  // Failed
        public const string CANCELLED = "Hủy";                    // Cancelled

        public static readonly string[] SuccessfulStatuses =
        {
            SUCCESS,
            LEGACY_COMPLETED,
            "Success",
            "Completed"
        };

        public static bool IsSuccessful(string? status) =>
            !string.IsNullOrWhiteSpace(status) &&
            SuccessfulStatuses.Contains(status.Trim(), StringComparer.OrdinalIgnoreCase);

        public static string Normalize(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return PENDING;

            var normalized = status.Trim();

            if (IsSuccessful(normalized))
                return SUCCESS;

            if (string.Equals(normalized, PENDING, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Pending", StringComparison.OrdinalIgnoreCase))
                return PENDING;

            if (string.Equals(normalized, FAILED, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Failed", StringComparison.OrdinalIgnoreCase))
                return FAILED;

            if (string.Equals(normalized, CANCELLED, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Canceled", StringComparison.OrdinalIgnoreCase))
                return CANCELLED;

            return normalized;
        }
    }

    /// <summary>
    /// Thông tin ngân hàng cho chuyển khoản
    /// </summary>
    public static class BankInfo
    {
        public const string BANK_NAME = BidvQrInfo.BANK_NAME;
        public const string BANK_ACCOUNT = BidvQrInfo.BANK_ACCOUNT;
        public const string ACCOUNT_HOLDER = BidvQrInfo.ACCOUNT_HOLDER;
    }

    /// <summary>
    /// Thông tin BIDV QR Pay
    /// </summary>
    public static class BidvQrInfo
    {
        public const string BANK_ID = "BIDV";
        public const string BANK_NAME = "BIDV";
        public const string BANK_ACCOUNT = "8823857657";
        public const string ACCOUNT_HOLDER = "LE VAN LOC";
        public const string QR_TEMPLATE = "compact2";
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
