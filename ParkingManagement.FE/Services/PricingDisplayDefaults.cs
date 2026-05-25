using ParkingManagement.FE.Models;

namespace ParkingManagement.FE.Services
{
    public static class PricingDisplayDefaults
    {
        public const string Motorcycle = "Xe máy";
        public const string SmallCar = "Ô tô nhỏ";
        public const string LargeCar = "Ô tô lớn";

        private static readonly Dictionary<string, decimal> DefaultHourlyRate = new()
        {
            [Motorcycle] = 3000m,
            [SmallCar] = 5000m,
            [LargeCar] = 8000m
        };

        private static readonly Dictionary<string, decimal> DefaultMaxDailyFee = new()
        {
            [Motorcycle] = 30000m,
            [SmallCar] = 50000m,
            [LargeCar] = 80000m
        };

        public static PricingDto CreateDefaultPricing()
        {
            return new PricingDto
            {
                HourlyRate = new Dictionary<string, decimal>(DefaultHourlyRate),
                MaxDailyFee = new Dictionary<string, decimal>(DefaultMaxDailyFee),
                MonthlyTicketPrice = new Dictionary<string, MonthlyPricingDto>
                {
                    [Motorcycle] = new() { OneMonth = 400000m, ThreeMonth = 1100000m, SixMonth = 2000000m },
                    [SmallCar] = new() { OneMonth = 1200000m, ThreeMonth = 3200000m, SixMonth = 6000000m },
                    [LargeCar] = new() { OneMonth = 2000000m, ThreeMonth = 5500000m, SixMonth = 10000000m }
                },
                LastUpdatedAt = DateTime.UtcNow
            };
        }

        public static decimal GetHourlyRate(PricingDto? pricing, string vehicleType)
        {
            return GetValue(pricing?.HourlyRate, DefaultHourlyRate, vehicleType);
        }

        public static decimal GetMaxDailyFee(PricingDto? pricing, string vehicleType)
        {
            return GetValue(pricing?.MaxDailyFee, DefaultMaxDailyFee, vehicleType);
        }

        public static decimal GetMonthlyTicketPrice(PricingDto? pricing, string vehicleType, int months)
        {
            var defaultPricing = CreateDefaultPricing();
            var monthlyPrice = GetMonthlyPrice(pricing?.MonthlyTicketPrice, vehicleType)
                ?? GetMonthlyPrice(defaultPricing.MonthlyTicketPrice, vehicleType);

            if (monthlyPrice == null)
            {
                return 0m;
            }

            return months switch
            {
                1 => monthlyPrice.OneMonth,
                3 => monthlyPrice.ThreeMonth,
                6 => monthlyPrice.SixMonth,
                _ => 0m
            };
        }

        private static decimal GetValue(
            Dictionary<string, decimal>? values,
            Dictionary<string, decimal> defaults,
            string vehicleType)
        {
            if (values != null)
            {
                if (values.TryGetValue(vehicleType, out var value))
                {
                    return value;
                }

                var matchingValue = values
                    .FirstOrDefault(item => string.Equals(item.Key, vehicleType, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(matchingValue.Key))
                {
                    return matchingValue.Value;
                }
            }

            return defaults.TryGetValue(vehicleType, out var fallback) ? fallback : 0m;
        }

        private static MonthlyPricingDto? GetMonthlyPrice(
            Dictionary<string, MonthlyPricingDto>? values,
            string vehicleType)
        {
            if (values == null)
            {
                return null;
            }

            if (values.TryGetValue(vehicleType, out var monthlyPrice))
            {
                return monthlyPrice;
            }

            return values
                .FirstOrDefault(item => string.Equals(item.Key, vehicleType, StringComparison.OrdinalIgnoreCase))
                .Value;
        }
    }
}
