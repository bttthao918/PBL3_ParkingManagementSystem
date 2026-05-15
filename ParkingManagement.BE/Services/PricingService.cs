using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Models;
using ParkingManagement.DAL.Repositories;

namespace ParkingManagement.BLL.Services.Implementations
{
    /// <summary>
    /// UC017 - Quản lý giá vé của Manager
    /// </summary>
    public class PricingService : IPricingService
    {
        private readonly IPricingRepository _pricingRepository;

        public PricingService(IPricingRepository pricingRepository)
        {
            _pricingRepository = pricingRepository;
        }

        /// <summary>
        /// UC017.1 - Lấy giá vé hiện tại
        /// </summary>
        public async Task<PricingDto> GetCurrentPricingAsync()
        {
            var pricingConfigs = await _pricingRepository.GetAllPricingConfigAsync();

            // Nếu chưa có giá, trả về giá mặc định
            if (!pricingConfigs.Any())
                return GetDefaultPricing();

            return MapToDto(pricingConfigs);
        }

        /// <summary>
        /// UC017.2 - Cập nhật giá vé
        /// </summary>
        public async Task<ServiceResult<PricingDto>> UpdatePricingAsync(UpdatePricingDto updateDto, string managerId)
        {
            try
            {
                // Validate input
                if (updateDto.HourlyRate == null && 
                    updateDto.MaxDailyFee == null && 
                    updateDto.MonthlyTicketPrice == null)
                {
                    return new ServiceResult<PricingDto>
                    {
                        Success = false,
                        Message = "Vui lòng cập nhật ít nhất một loại giá"
                    };
                }

                var currentPricing = await GetCurrentPricingAsync();
                updateDto.HourlyRate = MergeDecimalPricing(currentPricing.HourlyRate, updateDto.HourlyRate);
                updateDto.MaxDailyFee = MergeDecimalPricing(currentPricing.MaxDailyFee, updateDto.MaxDailyFee);
                updateDto.MonthlyTicketPrice = MergeMonthlyPricing(currentPricing.MonthlyTicketPrice, updateDto.MonthlyTicketPrice);

                // Validate amounts > 0
                if (updateDto.HourlyRate != null)
                {
                    if (updateDto.HourlyRate.Any(x => x.Value <= 0))
                        return new ServiceResult<PricingDto>
                        {
                            Success = false,
                            Message = "Giá theo giờ phải lớn hơn 0"
                        };
                }

                if (updateDto.MaxDailyFee != null)
                {
                    if (updateDto.MaxDailyFee.Any(x => x.Value <= 0))
                        return new ServiceResult<PricingDto>
                        {
                            Success = false,
                            Message = "Giá tối đa theo ngày phải lớn hơn 0"
                        };
                }

                if (updateDto.MonthlyTicketPrice != null)
                {
                    foreach (var monthlyPrice in updateDto.MonthlyTicketPrice.Values)
                    {
                        if ((monthlyPrice.OneMonth.HasValue && monthlyPrice.OneMonth <= 0) ||
                            (monthlyPrice.ThreeMonth.HasValue && monthlyPrice.ThreeMonth <= 0) ||
                            (monthlyPrice.SixMonth.HasValue && monthlyPrice.SixMonth <= 0))
                            return new ServiceResult<PricingDto>
                            {
                                Success = false,
                                Message = "Giá vé tháng phải lớn hơn 0"
                            };
                    }
                }

                // Xóa tất cả giá cũ
                await _pricingRepository.DeleteAllPricingAsync();

                var vehicleTypes = new[] { "Xe máy", "Ô tô nhỏ", "Ô tô lớn" };

                // Cập nhật hourly rate
                if (updateDto.HourlyRate != null)
                {
                    foreach (var vType in vehicleTypes)
                    {
                        if (updateDto.HourlyRate.TryGetValue(vType, out var rate))
                        {
                            await _pricingRepository.UpsertPricingAsync(new PricingConfiguration
                            {
                                PricingId = Guid.NewGuid().ToString(),
                                VehicleType = vType,
                                RateType = "HourlyRate",
                                Amount = rate,
                                UpdatedAt = DateTime.UtcNow,
                                UpdatedBy = managerId
                            });
                        }
                    }
                }

                // Cập nhật max daily fee
                if (updateDto.MaxDailyFee != null)
                {
                    foreach (var vType in vehicleTypes)
                    {
                        if (updateDto.MaxDailyFee.TryGetValue(vType, out var maxFee))
                        {
                            await _pricingRepository.UpsertPricingAsync(new PricingConfiguration
                            {
                                PricingId = Guid.NewGuid().ToString(),
                                VehicleType = vType,
                                RateType = "MaxDailyFee",
                                Amount = maxFee,
                                UpdatedAt = DateTime.UtcNow,
                                UpdatedBy = managerId
                            });
                        }
                    }
                }

                // Cập nhật monthly ticket pricing
                if (updateDto.MonthlyTicketPrice != null)
                {
                    foreach (var vType in vehicleTypes)
                    {
                        if (updateDto.MonthlyTicketPrice.TryGetValue(vType, out var monthlyPrice))
                        {
                            if (monthlyPrice.OneMonth.HasValue)
                            {
                                await _pricingRepository.UpsertPricingAsync(new PricingConfiguration
                                {
                                    PricingId = Guid.NewGuid().ToString(),
                                    VehicleType = vType,
                                    RateType = "Monthly1M",
                                    Amount = monthlyPrice.OneMonth.Value,
                                    UpdatedAt = DateTime.UtcNow,
                                    UpdatedBy = managerId
                                });
                            }

                            if (monthlyPrice.ThreeMonth.HasValue)
                            {
                                await _pricingRepository.UpsertPricingAsync(new PricingConfiguration
                                {
                                    PricingId = Guid.NewGuid().ToString(),
                                    VehicleType = vType,
                                    RateType = "Monthly3M",
                                    Amount = monthlyPrice.ThreeMonth.Value,
                                    UpdatedAt = DateTime.UtcNow,
                                    UpdatedBy = managerId
                                });
                            }

                            if (monthlyPrice.SixMonth.HasValue)
                            {
                                await _pricingRepository.UpsertPricingAsync(new PricingConfiguration
                                {
                                    PricingId = Guid.NewGuid().ToString(),
                                    VehicleType = vType,
                                    RateType = "Monthly6M",
                                    Amount = monthlyPrice.SixMonth.Value,
                                    UpdatedAt = DateTime.UtcNow,
                                    UpdatedBy = managerId
                                });
                            }
                        }
                    }
                }

                // Lấy giá đã cập nhật
                var updatedPricing = await GetCurrentPricingAsync();

                return new ServiceResult<PricingDto>
                {
                    Success = true,
                    Message = "Cập nhật giá vé thành công!",
                    Data = updatedPricing
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<PricingDto>
                {
                    Success = false,
                    Message = $"Lỗi cập nhật giá vé: {ex.Message}"
                };
            }
        }

        public async Task<decimal> GetMonthlyTicketPriceAsync(string vehicleType, int months)
        {
            var pricing = await GetCurrentPricingAsync();
            if (!pricing.MonthlyTicketPrice.TryGetValue(vehicleType, out var monthlyPrice))
            {
                var matchingPrice = pricing.MonthlyTicketPrice
                    .FirstOrDefault(item => string.Equals(item.Key, vehicleType, StringComparison.OrdinalIgnoreCase));

                monthlyPrice = matchingPrice.Value;
            }

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

        private static Dictionary<string, decimal> MergeDecimalPricing(
            Dictionary<string, decimal> currentPricing,
            Dictionary<string, decimal>? updatedPricing)
        {
            var mergedPricing = new Dictionary<string, decimal>(currentPricing);

            if (updatedPricing == null)
                return mergedPricing;

            foreach (var item in updatedPricing)
            {
                mergedPricing[item.Key] = item.Value;
            }

            return mergedPricing;
        }

        private static Dictionary<string, UpdateMonthlyPricingDto> MergeMonthlyPricing(
            Dictionary<string, MonthlyPricingDto> currentPricing,
            Dictionary<string, UpdateMonthlyPricingDto>? updatedPricing)
        {
            var mergedPricing = currentPricing.ToDictionary(
                item => item.Key,
                item => new UpdateMonthlyPricingDto
                {
                    OneMonth = item.Value.OneMonth,
                    ThreeMonth = item.Value.ThreeMonth,
                    SixMonth = item.Value.SixMonth
                });

            if (updatedPricing == null)
                return mergedPricing;

            foreach (var item in updatedPricing)
            {
                if (!mergedPricing.TryGetValue(item.Key, out var monthlyPricing))
                {
                    monthlyPricing = new UpdateMonthlyPricingDto();
                    mergedPricing[item.Key] = monthlyPricing;
                }

                if (item.Value.OneMonth.HasValue)
                    monthlyPricing.OneMonth = item.Value.OneMonth;

                if (item.Value.ThreeMonth.HasValue)
                    monthlyPricing.ThreeMonth = item.Value.ThreeMonth;

                if (item.Value.SixMonth.HasValue)
                    monthlyPricing.SixMonth = item.Value.SixMonth;
            }

            return mergedPricing;
        }

        /// <summary>
        /// Map from PricingConfiguration list to PricingDto
        /// </summary>
        private PricingDto MapToDto(List<PricingConfiguration> configs)
        {
            var defaultPricing = GetDefaultPricing();
            var hourlyRate = new Dictionary<string, decimal>(defaultPricing.HourlyRate);
            var maxDailyFee = new Dictionary<string, decimal>(defaultPricing.MaxDailyFee);
            var monthlyTicketPrice = defaultPricing.MonthlyTicketPrice.ToDictionary(
                item => item.Key,
                item => new MonthlyPricingDto
                {
                    OneMonth = item.Value.OneMonth,
                    ThreeMonth = item.Value.ThreeMonth,
                    SixMonth = item.Value.SixMonth
                });

            var vehicleTypes = new[] { "Xe máy", "Ô tô nhỏ", "Ô tô lớn" };

            foreach (var vType in vehicleTypes)
            {
                // Hourly rate
                var hourlyConfig = configs.FirstOrDefault(p => p.VehicleType == vType && p.RateType == "HourlyRate");
                if (hourlyConfig != null)
                    hourlyRate[vType] = hourlyConfig.Amount;

                // Max daily fee
                var maxDailyConfig = configs.FirstOrDefault(p => p.VehicleType == vType && p.RateType == "MaxDailyFee");
                if (maxDailyConfig != null)
                    maxDailyFee[vType] = maxDailyConfig.Amount;

                // Monthly pricing
                var monthly1M = configs.FirstOrDefault(p => p.VehicleType == vType && p.RateType == "Monthly1M");
                var monthly3M = configs.FirstOrDefault(p => p.VehicleType == vType && p.RateType == "Monthly3M");
                var monthly6M = configs.FirstOrDefault(p => p.VehicleType == vType && p.RateType == "Monthly6M");

                if (monthly1M != null || monthly3M != null || monthly6M != null)
                {
                    monthlyTicketPrice.TryGetValue(vType, out var currentMonthlyPrice);

                    monthlyTicketPrice[vType] = new MonthlyPricingDto
                    {
                        OneMonth = monthly1M?.Amount ?? currentMonthlyPrice?.OneMonth ?? 0,
                        ThreeMonth = monthly3M?.Amount ?? currentMonthlyPrice?.ThreeMonth ?? 0,
                        SixMonth = monthly6M?.Amount ?? currentMonthlyPrice?.SixMonth ?? 0
                    };
                }
            }

            var lastUpdated = configs.OrderByDescending(p => p.UpdatedAt).FirstOrDefault();

            return new PricingDto
            {
                HourlyRate = hourlyRate,
                MaxDailyFee = maxDailyFee,
                MonthlyTicketPrice = monthlyTicketPrice,
                LastUpdatedAt = lastUpdated?.UpdatedAt ?? DateTime.UtcNow,
                LastUpdatedBy = lastUpdated?.UpdatedBy
            };
        }

        /// <summary>
        /// Lấy giá mặc định (nếu chưa cập nhật)
        /// </summary>
        private PricingDto GetDefaultPricing()
        {
            return new PricingDto
            {
                HourlyRate = new Dictionary<string, decimal>
                {
                    { "Xe máy", 3000m },
                    { "Ô tô nhỏ", 5000m },
                    { "Ô tô lớn", 8000m }
                },
                MaxDailyFee = new Dictionary<string, decimal>
                {
                    { "Xe máy", 30000m },
                    { "Ô tô nhỏ", 50000m },
                    { "Ô tô lớn", 80000m }
                },
                MonthlyTicketPrice = new Dictionary<string, MonthlyPricingDto>
                {
                    {
                        "Xe máy",
                        new MonthlyPricingDto
                        {
                            OneMonth = 150000m,
                            ThreeMonth = 400000m,
                            SixMonth = 750000m
                        }
                    },
                    {
                        "Ô tô nhỏ",
                        new MonthlyPricingDto
                        {
                            OneMonth = 300000m,
                            ThreeMonth = 800000m,
                            SixMonth = 1500000m
                        }
                    },
                    {
                        "Ô tô lớn",
                        new MonthlyPricingDto
                        {
                            OneMonth = 500000m,
                            ThreeMonth = 1300000m,
                            SixMonth = 2500000m
                        }
                    }
                },
                LastUpdatedAt = DateTime.UtcNow,
                LastUpdatedBy = null
            };
        }
    }
}
