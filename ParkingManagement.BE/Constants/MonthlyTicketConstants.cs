namespace ParkingManagement.BLL.Constants
{
    public static class MonthlyTicketStatuses
    {
        public const string PENDING_PAYMENT = "Chờ thanh toán";
        public const string ACTIVE = "Hoạt động";
        public const string EXPIRED = "Hết hạn";
        public const string CANCELLED = "Đã hủy";

        public static bool IsActive(string? status) =>
            string.Equals(status, ACTIVE, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase);

        public static bool BlocksNewRegistration(string? status) =>
            string.Equals(status, ACTIVE, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, PENDING_PAYMENT, StringComparison.OrdinalIgnoreCase);
    }
}
