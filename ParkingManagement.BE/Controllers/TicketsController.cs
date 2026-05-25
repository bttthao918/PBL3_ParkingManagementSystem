using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.BLL.Constants;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;
using ParkingManagement.DAL.Data;
using ParkingManagement.DAL.Models;

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
        private readonly IPlateRecognitionService _plateRecognitionService;
        private readonly AppDbContext _db;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(
            ITicketService ticketService,
            IPlateRecognitionService plateRecognitionService,
            AppDbContext db,
            ILogger<TicketsController> logger)
        {
            _ticketService = ticketService;
            _plateRecognitionService = plateRecognitionService;
            _db = db;
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
                    VehiclePlate = validation.VehiclePlate ?? input.VehiclePlate,
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

                var shiftCheck = await ValidateEmployeeCanOperateNowAsync("check-in");
                if (!shiftCheck.CanOperate)
                {
                    return BadRequest(new CheckInResultDto
                    {
                        Success = false,
                        Message = shiftCheck.Message
                    });
                }

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

                var shiftCheck = await ValidateEmployeeCanOperateNowAsync("check-in");
                if (!shiftCheck.CanOperate)
                {
                    return Ok(new CheckInValidationDto
                    {
                        VehiclePlate = input.VehiclePlate,
                        OriginalVehiclePlate = input.VehiclePlate,
                        Message = shiftCheck.Message
                    });
                }

                var result = await _ticketService.ValidateAndPrepareCheckInAsync(input);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ValidateCheckIn error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpGet("plate-candidates")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlateCandidates()
        {
            try
            {
                var plates = await _ticketService.GetKnownVehiclePlatesAsync();
                return Ok(plates);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetPlateCandidates error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        [HttpPost("plate-recognition")]
        [Authorize(Roles = "Employee")]
        [ProducesResponseType(typeof(PlateRecognitionResponseDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> RecognizePlate([FromBody] PlateRecognitionRequestDto request)
        {
            try
            {
                var result = await _plateRecognitionService.RecognizeAsync(request, HttpContext.RequestAborted);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"RecognizePlate error: {ex.Message}");
                return StatusCode(500, new PlateRecognitionResponseDto
                {
                    Success = false,
                    Message = "Internal server error"
                });
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
                var shiftCheck = await ValidateEmployeeCanOperateNowAsync("check-out");
                if (!shiftCheck.CanOperate)
                {
                    return BadRequest(new CheckOutResultDto
                    {
                        Success = false,
                        Message = shiftCheck.Message
                    });
                }

                input.TicketId = ticketId;
                input.CollectedByEmployeeId = GetEmployeeId();
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

                var shiftCheck = await ValidateEmployeeCanOperateNowAsync("check-out");
                if (!shiftCheck.CanOperate)
                {
                    return Ok(new CheckOutValidationDto
                    {
                        Success = false,
                        Message = shiftCheck.Message
                    });
                }

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

        private async Task<(bool CanOperate, string Message)> ValidateEmployeeCanOperateNowAsync(string actionName)
        {
            var employeeId = GetEmployeeId();
            if (string.IsNullOrWhiteSpace(employeeId))
                return (false, "Không xác định được nhân viên đang đăng nhập.");

            var now = DateTime.Now;
            var activeLog = await _db.WorkLogs
                .Include(w => w.ShiftSchedule)
                .Where(w => w.EmployeeId == employeeId && w.Status == ShiftConstants.WorkingStatus)
                .OrderByDescending(w => w.StartTime)
                .FirstOrDefaultAsync();

            if (activeLog == null)
                return (false, $"Bạn cần bắt đầu ca làm trước khi {actionName} xe.");

            if (activeLog.WorkDate.Date != now.Date || activeLog.StartTime.Date != now.Date)
                return (false, $"Ca làm đang mở không thuộc ngày hôm nay. Vui lòng kết thúc ca cũ và bắt đầu ca hôm nay trước khi {actionName} xe.");

            if ((now - activeLog.StartTime).TotalHours > 12)
                return (false, $"Ca làm đang mở đã quá hạn. Vui lòng kết thúc ca cũ và bắt đầu ca mới trước khi {actionName} xe.");

            var schedule = activeLog.ShiftSchedule;
            if (schedule == null)
                return (true, string.Empty);

            var window = ShiftConstants.GetEffectiveWindow(schedule.ShiftType, schedule.StartTime, schedule.EndTime);
            if (ShiftConstants.IsWithinShift(now.TimeOfDay, window.Start, window.End))
                return (true, string.Empty);

            var shiftText = $"{schedule.ShiftType} {ShiftConstants.FormatWindow(window.Start, window.End)}";
            return (false, $"Hiện tại {now:HH:mm} không nằm trong ca đang làm ({shiftText}). Bạn chỉ có thể {actionName} xe trong ca làm hiện tại.");
        }

        private string? GetEmployeeId()
        {
            return User.FindFirst("employeeId")?.Value
                ?? User.FindFirst("related_id")?.Value;
        }
    }
}
