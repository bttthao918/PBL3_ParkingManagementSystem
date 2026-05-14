using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.Web.Controllers.Api
{
    /// <summary>
    /// API for Ticket Management
    /// Handles check-in, check-out, and ticket history
    /// </summary>
    [ApiController]
    [Route("api/tickets")]
    [Authorize]
    [Produces("application/json")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(
            ITicketService ticketService,
            ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _logger = logger;
        }

        /// <summary>
        /// Get all tickets with filtering and pagination
        /// </summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ListTicketDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] TicketFilterDto filter)
        {
            try
            {
                var result = await _ticketService.GetTicketsAsync(filter);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetAll error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get ticket summary directly from database
        /// </summary>
        [HttpGet("summary")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TicketSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var result = await _ticketService.GetTicketSummaryAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetSummary error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get ticket detail by ID
        /// </summary>
        [HttpGet("{ticketId}")]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string ticketId)
        {
            try
            {
                var ticket = await _ticketService.GetTicketDetailAsync(ticketId);
                if (ticket == null)
                    return NotFound(new { message = "Khong tim thay ve" });

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetById error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update ticket information (Manager/Admin)
        /// </summary>
        [HttpPut("{ticketId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(TicketDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(string ticketId, [FromBody] UpdateTicketDto input)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ticket = await _ticketService.UpdateTicketAsync(ticketId, input);
                return Ok(ticket);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Update error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Delete ticket (Manager/Admin)
        /// </summary>
        [HttpDelete("{ticketId}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(string ticketId)
        {
            try
            {
                var deleted = await _ticketService.DeleteTicketAsync(ticketId);
                if (!deleted)
                    return NotFound(new { message = "Khong tim thay ve" });

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Delete error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create a new parking ticket from the manager/admin page.
        /// </summary>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CheckInResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateTicketDto input)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrWhiteSpace(input.VehiclePlate) || string.IsNullOrWhiteSpace(input.VehicleType))
                {
                    return BadRequest(new CheckInResultDto
                    {
                        Success = false,
                        Message = "Vui lòng nhập biển số xe và loại xe."
                    });
                }

                var validation = await _ticketService.ValidateAndPrepareCheckInAsync(new CheckInInputDto
                {
                    VehiclePlate = input.VehiclePlate,
                    VehicleType = input.VehicleType,
                    CustomerId = input.CustomerId
                });

                var slotId = string.IsNullOrWhiteSpace(input.SlotId)
                    ? validation.PreferredSlotId ?? validation.AvailableSlots.FirstOrDefault()?.SlotId
                    : input.SlotId.Trim();

                if (string.IsNullOrWhiteSpace(slotId))
                {
                    return BadRequest(new CheckInResultDto
                    {
                        Success = false,
                        Message = validation.Message ?? "Không còn chỗ trống phù hợp để tạo vé."
                    });
                }

                var result = await _ticketService.ConfirmCheckInAsync(new ConfirmCheckInDto
                {
                    VehiclePlate = input.VehiclePlate,
                    VehicleType = input.VehicleType,
                    SlotId = slotId,
                    CustomerId = validation.CustomerId ?? input.CustomerId
                });

                if (!result.Success)
                    return BadRequest(result);

                _logger.LogInformation($"Ticket created from admin page: {result.TicketId}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Create ticket error: {ex.Message}");
                return StatusCode(500, new CheckInResultDto
                {
                    Success = false,
                    Message = "Internal server error"
                });
            }
        }

        /// <summary>
        /// Check-in vehicle (Employee only)
        /// </summary>
        [HttpPost("checkin")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(CheckInResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckIn([FromBody] ConfirmCheckInDto input)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _ticketService.ConfirmCheckInAsync(input);
                if (!result.Success)
                    return BadRequest(result);

                _logger.LogInformation($"Vehicle checked in: {input.VehiclePlate}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CheckIn error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate check-in: kiểm tra biển số, vé tháng, đặt chỗ, gợi ý slot trống (Employee only)
        /// </summary>
        [HttpPost("checkin/validate")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(CheckInValidationDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateCheckIn([FromBody] CheckInInputDto input)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _ticketService.ValidateAndPrepareCheckInAsync(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ValidateCheckIn error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check-out vehicle and process payment (Employee only)
        /// </summary>
        [HttpPost("{ticketId}/checkout")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(CheckOutResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CheckOut(string ticketId, [FromBody] ConfirmCheckOutDto input)
        {
            try
            {
                input.TicketId = ticketId;
                var result = await _ticketService.ConfirmCheckOutAsync(input);
                if (!result.Success)
                    return BadRequest(result);

                _logger.LogInformation($"Vehicle checked out: {ticketId}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CheckOut error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate check-out: tính phí, kiểm tra vé tháng (Employee only)
        /// </summary>
        [HttpPost("checkout/validate")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(CheckOutValidationDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateCheckOut([FromBody] CheckOutInputDto input)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _ticketService.ValidateAndPrepareCheckOutAsync(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ValidateCheckOut error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Search tickets (Employee only)
        /// </summary>
        [HttpGet("search")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(ListEmployeeTicketDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] EmployeeTicketSearchDto search)
        {
            try
            {
                var result = await _ticketService.SearchTicketsAsync(search);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Search error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
