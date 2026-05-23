namespace ParkingManagement.BLL.Helpers
{
    public static class VipHelper
    {
        public const string MEMBER = "Thành viên";
        public const string SILVER = "Bạc";
        public const string GOLD = "Vàng";
        public const string PLATINUM = "Kim Cương";

        public const decimal SILVER_THRESHOLD = 2000000;
        public const decimal GOLD_THRESHOLD = 5000000;
        public const decimal PLATINUM_THRESHOLD = 10000000;

        public static int GetVipDiscountPercent(string? vipLevel)
        {
            return vipLevel switch
            {
                PLATINUM => 15,
                GOLD => 10,
                SILVER => 5,
                _ => 0
            };
        }

        public static string DetermineVipLevel(decimal totalSpent)
        {
            if (totalSpent >= PLATINUM_THRESHOLD)
                return PLATINUM;
            if (totalSpent >= GOLD_THRESHOLD)
                return GOLD;
            if (totalSpent >= SILVER_THRESHOLD)
                return SILVER;
            return MEMBER;
        }

        public static void CalculateProgress(decimal totalSpent, out int progress, out decimal amountToNext)
        {
            progress = 0;
            amountToNext = 0;

            if (totalSpent < SILVER_THRESHOLD)
            {
                progress = (int)(totalSpent / SILVER_THRESHOLD * 100);
                amountToNext = SILVER_THRESHOLD - totalSpent;
            }
            else if (totalSpent < GOLD_THRESHOLD)
            {
                progress = (int)((totalSpent - SILVER_THRESHOLD) / (GOLD_THRESHOLD - SILVER_THRESHOLD) * 100);
                amountToNext = GOLD_THRESHOLD - totalSpent;
            }
            else if (totalSpent < PLATINUM_THRESHOLD)
            {
                progress = (int)((totalSpent - GOLD_THRESHOLD) / (PLATINUM_THRESHOLD - GOLD_THRESHOLD) * 100);
                amountToNext = PLATINUM_THRESHOLD - totalSpent;
            }
            else
            {
                progress = 100;
                amountToNext = 0;
            }
        }
    }
}
