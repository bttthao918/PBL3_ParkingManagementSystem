using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.Web.Controllers.Api
{
    /// <summary>
    /// API for Pricing Management
    /// Manage pricing rules for parking tickets and monthly packages
    /// </summary>
    [ApiController]
    [Route("api/pricing")]
    [Authorize]
    [Produces("application/json")]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;

        public PricingController(IPricingService pricingService)
        {
            _pricingService = pricingService;
        }

        /// <summary>
        /// Get all pricing rules
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PricingDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var pricing = await _pricingService.GetCurrentPricingAsync();
            return Ok(pricing);
        }

        /// <summary>
        /// Get pricing by ID
        /// </summary>
        [HttpGet("{pricingId}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string pricingId)
        {
            // TODO: Implement GetByIdAsync in service
            return StatusCode(StatusCodes.Status501NotImplemented, "Not yet implemented");
        }

        /// <summary>
        /// Calculate ticket fee for a parking duration
        /// </summary>
        [HttpPost("calculate-ticket")]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<decimal>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CalculateTicketFee(
            [FromQuery] string vehicleType,
            [FromQuery] int durationMinutes)
        {
            // TODO: Implement CalculateTicketFeeAsync in service
            return StatusCode(StatusCodes.Status501NotImplemented, "Not yet implemented");
        }

        /// <summary>
        /// Get monthly ticket pricing
        /// </summary>
        [HttpGet("monthly")]
        [ProducesResponseType(typeof(Dictionary<string, MonthlyPricingDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMonthlyPricing()
        {
            var pricing = await _pricingService.GetCurrentPricingAsync();
            return Ok(pricing.MonthlyTicketPrice);
        }

        /// <summary>
        /// Get pricing by vehicle type
        /// </summary>
        [HttpGet("vehicle/{vehicleType}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByVehicleType(string vehicleType)
        {
            var pricing = await _pricingService.GetCurrentPricingAsync();
            return Ok(new
            {
                VehicleType = vehicleType,
                HourlyRate = pricing.HourlyRate.TryGetValue(vehicleType, out var hourlyRate) ? hourlyRate : 0,
                MaxDailyFee = pricing.MaxDailyFee.TryGetValue(vehicleType, out var maxDailyFee) ? maxDailyFee : 0,
                MonthlyTicketPrice = pricing.MonthlyTicketPrice.TryGetValue(vehicleType, out var monthlyPrice) ? monthlyPrice : null
            });
        }

        /// <summary>
        /// Create or update pricing rule
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(ServiceResult<PricingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<PricingDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrUpdate([FromBody] UpdatePricingDto pricingRequest)
        {
            return await UpdateCurrentPricing(pricingRequest);
        }

        /// <summary>
        /// Update current active pricing
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "Manager,Admin")]
        [ProducesResponseType(typeof(ServiceResult<PricingDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<PricingDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCurrent([FromBody] UpdatePricingDto pricingUpdate)
        {
            return await UpdateCurrentPricing(pricingUpdate);
        }

        /// <summary>
        /// Update pricing rule
        /// </summary>
        [HttpPut("{pricingId}")]
        [ProducesResponseType(typeof(ServiceResult<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(string pricingId, [FromBody] object pricingUpdate)
        {
            // TODO: Implement UpdateAsync in service
            return StatusCode(StatusCodes.Status501NotImplemented, "Not yet implemented");
        }

        /// <summary>
        /// Delete pricing rule
        /// </summary>
        [HttpDelete("{pricingId}")]
        [ProducesResponseType(typeof(ServiceResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(string pricingId)
        {
            // TODO: Implement DeleteAsync in service
            return StatusCode(StatusCodes.Status501NotImplemented, "Not yet implemented");
        }

        /// <summary>
        /// Get current active pricing
        /// </summary>
        [HttpGet("active")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PricingDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActivePricing()
        {
            var pricing = await _pricingService.GetCurrentPricingAsync();
            return Ok(pricing);
        }

        private async Task<IActionResult> UpdateCurrentPricing(UpdatePricingDto pricingUpdate)
        {
            if (pricingUpdate == null)
                return BadRequest(ServiceResult<PricingDto>.Fail("Du lieu cap nhat gia ve khong hop le."));

            var managerId = User.FindFirst("managerId")?.Value
                ?? User.FindFirst("accountId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? "System";

            var result = await _pricingService.UpdatePricingAsync(pricingUpdate, managerId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
