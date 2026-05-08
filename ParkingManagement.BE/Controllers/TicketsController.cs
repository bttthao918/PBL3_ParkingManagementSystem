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
