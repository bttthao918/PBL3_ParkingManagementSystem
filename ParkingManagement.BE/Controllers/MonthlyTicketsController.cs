using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.Web.Controllers.Api
{
    [ApiController]
    [Route("api/monthly-tickets")]
    [Authorize]
    [Produces("application/json")]
    public class MonthlyTicketsController : ControllerBase
    {
        private readonly IMonthlyTicketService _monthlyTicketService;
        private readonly IPricingService _pricingService;
        private readonly ILogger<MonthlyTicketsController> _logger;

        public MonthlyTicketsController(
            IMonthlyTicketService monthlyTicketService,
            IPricingService pricingService,
            ILogger<MonthlyTicketsController> logger)
        {
            _monthlyTicketService = monthlyTicketService;
            _pricingService = pricingService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ListMonthlyTicketsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                var monthlyTickets = await _monthlyTicketService.GetByCustomerIdAsync(customerId);

                var response = new ListMonthlyTicketsDto
                {
                    Items = monthlyTickets.Select(ToDetailDto).ToList(),
                    ActiveCount = monthlyTickets.Count(IsActive),
                    ExpiredCount = monthlyTickets.Count(x => !IsActive(x))
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAll error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("{monthlyTicketId}")]
        [ProducesResponseType(typeof(MonthlyTicketDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string monthlyTicketId)
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                var ticket = await _monthlyTicketService.GetByIdAsync(monthlyTicketId);
                if (ticket == null)
                    return NotFound(new { message = "Monthly ticket not found" });

                return Ok(ToDetailDto(ticket));
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetById error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegisterMonthlyTicketResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] RegisterMonthlyTicketDto dto)
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                dto.CustomerId = customerId;
                ModelState.Remove(nameof(RegisterMonthlyTicketDto.CustomerId));

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!IsValidPackage(dto.PackageType))
                    return BadRequest(new { message = "Invalid package type. Must be '1 tháng', '3 tháng', or '6 tháng'" });

                var result = await _monthlyTicketService.RegisterAsync(dto);
                if (!result.Success)
                    return BadRequest(result);

                var monthlyTicket = ToDetailDto(result.Data!);
                var response = new RegisterMonthlyTicketResponseDto
                {
                    Success = true,
                    Message = result.Message ?? "Monthly ticket registered successfully.",
                    Fee = result.Data!.TotalFee,
                    OrderCode = result.Data.PayOsOrderCode,
                    PaymentLinkId = result.Data.PayOsPaymentLinkId,
                    CheckoutUrl = result.Data.CheckoutUrl,
                    QrCode = result.Data.QrCode,
                    Data = monthlyTicket
                };

                _logger.LogInformation($"Monthly ticket registered: {monthlyTicket.MonthlyTicketId}");
                return CreatedAtAction(nameof(GetById), new { monthlyTicketId = monthlyTicket.MonthlyTicketId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create monthly ticket error");
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost("{monthlyTicketId}/renew")]
        [ProducesResponseType(typeof(RenewMonthlyTicketResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Renew(string monthlyTicketId, [FromBody] RenewMonthlyTicketDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                if (!IsValidPackage(dto.PackageType))
                    return BadRequest(new { message = "Invalid package type" });

                var result = await _monthlyTicketService.RenewAsync(monthlyTicketId, dto);
                if (!result.Success)
                    return BadRequest(result);

                var response = new RenewMonthlyTicketResponseDto
                {
                    Success = true,
                    Message = result.Message ?? "Monthly ticket renewed successfully.",
                    AdditionalFee = await _monthlyTicketService.CalculateFeeAsync(result.Data!.VehicleType, dto.PackageType),
                    Data = ToDetailDto(result.Data)
                };

                _logger.LogInformation($"Monthly ticket renewed: {monthlyTicketId}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Renew error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("{monthlyTicketId}/payment-link")]
        [ProducesResponseType(typeof(RegisterMonthlyTicketResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePaymentLink(string monthlyTicketId)
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                var result = await _monthlyTicketService.CreatePendingPayOsPaymentAsync(monthlyTicketId, customerId);
                if (!result.Success)
                    return BadRequest(result);

                var monthlyTicket = ToDetailDto(result.Data!);
                var response = new RegisterMonthlyTicketResponseDto
                {
                    Success = true,
                    Message = result.Message ?? "Payment QR created successfully.",
                    Fee = result.Data!.TotalFee,
                    OrderCode = result.Data.PayOsOrderCode,
                    PaymentLinkId = result.Data.PayOsPaymentLinkId,
                    CheckoutUrl = result.Data.CheckoutUrl,
                    QrCode = result.Data.QrCode,
                    Data = monthlyTicket
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create payment link error");
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpDelete("{monthlyTicketId}")]
        [ProducesResponseType(typeof(CancelReservationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancel(string monthlyTicketId)
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                var result = await _monthlyTicketService.CancelAsync(monthlyTicketId);
                if (!result.Success)
                    return BadRequest(result);

                var response = new CancelReservationDto
                {
                    Success = true,
                    Message = result.Message ?? "Monthly ticket cancelled successfully"
                };

                _logger.LogInformation($"Monthly ticket cancelled: {monthlyTicketId}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Cancel error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("pricing")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MonthlyTicketPricingDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPricing([FromQuery] string vehicleType = "Xe máy")
        {
            var pricing = await _pricingService.GetCurrentPricingAsync();
            var monthlyPrice = GetMonthlyPricingForVehicle(pricing, vehicleType);

            return Ok(new MonthlyTicketPricingDto
            {
                Packages = new List<PackagePriceDto>
                {
                    new() { Package = "1 tháng", Price = monthlyPrice.OneMonth },
                    new() { Package = "3 tháng", Price = monthlyPrice.ThreeMonth, Discount = FormatDiscount(monthlyPrice.OneMonth, monthlyPrice.ThreeMonth, 3) },
                    new() { Package = "6 tháng", Price = monthlyPrice.SixMonth, Discount = FormatDiscount(monthlyPrice.OneMonth, monthlyPrice.SixMonth, 6) }
                }
            });
        }

        private static string FormatDiscount(decimal oneMonthPrice, decimal packagePrice, int months)
        {
            var fullPrice = oneMonthPrice * months;
            if (fullPrice <= 0 || packagePrice <= 0 || packagePrice >= fullPrice)
            {
                return "0%";
            }

            var discount = (1m - packagePrice / fullPrice) * 100m;
            return $"{Math.Round(discount, 2, MidpointRounding.AwayFromZero):0.##}%";
        }

        private static bool IsValidPackage(string packageType)
            => new[] { "1 tháng", "3 tháng", "6 tháng" }.Contains(packageType);

        private static bool IsActive(MonthlyTicketDto ticket)
            => ticket.Status == "Hoạt động" || ticket.Status == "Active";

        private static MonthlyPricingDto GetMonthlyPricingForVehicle(PricingDto pricing, string vehicleType)
        {
            if (pricing.MonthlyTicketPrice.TryGetValue(vehicleType, out var monthlyPrice))
            {
                return monthlyPrice;
            }

            var matchingPrice = pricing.MonthlyTicketPrice
                .FirstOrDefault(item => string.Equals(item.Key, vehicleType, StringComparison.OrdinalIgnoreCase));

            return matchingPrice.Value ?? new MonthlyPricingDto();
        }

        private static MonthlyTicketDetailDto ToDetailDto(MonthlyTicketDto ticket) => new()
        {
            MonthlyTicketId = ticket.MonthlyTicketId,
            VehiclePlate = ticket.VehiclePlate,
            VehicleType = ticket.VehicleType,
            PackageType = ticket.PackageType,
            StartDate = ticket.StartDate,
            EndDate = ticket.EndDate,
            TotalFee = ticket.TotalFee,
            Status = ticket.Status,
            DaysRemaining = ticket.DaysRemaining
        };
    }

    public class MonthlyTicketPricingDto
    {
        public List<PackagePriceDto> Packages { get; set; } = new();
    }

    public class PackagePriceDto
    {
        public string Package { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Discount { get; set; }
    }
}
