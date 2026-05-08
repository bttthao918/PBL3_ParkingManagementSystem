using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.Web.Controllers.Api
{
    /// <summary>
    /// API for Parking Slot Management
    /// Includes: General operations, Manager operations, and Employee operations
    /// </summary>
    [ApiController]
    [Route("api/parking-slots")]
    [Produces("application/json")]
    public class ParkingSlotsController : ControllerBase
    {
        private readonly IParkingSlotService _parkingSlotService;

        public ParkingSlotsController(IParkingSlotService parkingSlotService)
        {
            _parkingSlotService = parkingSlotService;
        }

        // ── 1. General Slot Management ──

        /// <summary>
        /// Get all parking slots
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ParkingSlotDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _parkingSlotService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get available slots by vehicle type
        /// </summary>
        [HttpGet("available/{vehicleType}")]
        [ProducesResponseType(typeof(List<ParkingSlotDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailable(string vehicleType)
        {
            var result = await _parkingSlotService.GetAvailableAsync(vehicleType);
            return Ok(result);
        }

        /// <summary>
        /// Get slot by ID
        /// </summary>
        [HttpGet("{slotId}")]
        [ProducesResponseType(typeof(ParkingSlotDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string slotId)
        {
            var result = await _parkingSlotService.GetByIdAsync(slotId);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy chỗ đỗ" });
            return Ok(result);
        }

        /// <summary>
        /// Update slot status (check validation first)
        /// </summary>
        [HttpPatch("{slotId}/status")]
        [ProducesResponseType(typeof(ServiceResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateStatus(string slotId, [FromBody] UpdateSlotStatusDto dto, [FromQuery] string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return BadRequest(new { message = "Cần cung cấp employeeId" });

            dto.SlotId = slotId;
            var result = await _parkingSlotService.UpdateStatusAsync(dto, employeeId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get slot summary (counts by status)
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(Dictionary<string, int>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _parkingSlotService.GetSlotSummaryAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get audit logs for a specific slot
        /// </summary>
        [HttpGet("{slotId}/audit-logs")]
        [ProducesResponseType(typeof(List<dynamic>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogs(string slotId)
        {
            var result = await _parkingSlotService.GetAuditLogsAsync(slotId);
            return Ok(result);
        }

        /// <summary>
        /// Get audit logs for an employee
        /// </summary>
        [HttpGet("audit-logs/employee/{employeeId}")]
        [ProducesResponseType(typeof(List<dynamic>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmployeeAuditLogs(string employeeId, [FromQuery] int days = 30)
        {
            var result = await _parkingSlotService.GetEmployeeAuditLogsAsync(employeeId, days);
            return Ok(result);
        }

        /// <summary>
        /// Check if status transition is allowed (validation only)
        /// </summary>
        [HttpPost("validate-transition")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateTransition([FromQuery] string slotId, [FromQuery] string newStatus)
        {
            var result = await _parkingSlotService.CanUpdateStatusAsync(slotId, newStatus);
            return Ok(result);
        }

        // ── 2. Manager Slot Management ──

        /// <summary>
        /// Get parking slots list (filtered, paginated) - Manager view
        /// </summary>
        [HttpGet("manager/list")]
        [ProducesResponseType(typeof(ListParkingSlotDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetForManager([FromQuery] ParkingSlotFilterDto filter)
        {
            var result = await _parkingSlotService.GetParkingSlotsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Get slot detail with usage statistics - Manager view
        /// </summary>
        [HttpGet("manager/{slotId}/detail")]
        [ProducesResponseType(typeof(ParkingSlotDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetailForManager(string slotId)
        {
            try
            {
                var result = await _parkingSlotService.GetParkingSlotDetailAsync(slotId);
                return Ok(result);
            }
            catch
            {
                return NotFound(new { message = "Không tìm thấy chỗ đỗ" });
            }
        }

        /// <summary>
        /// Get parking slot report - Manager view
        /// </summary>
        [HttpGet("manager/report")]
        [ProducesResponseType(typeof(ParkingSlotReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReportForManager()
        {
            var result = await _parkingSlotService.GetParkingSlotReportAsync();
            return Ok(result);
        }

        /// <summary>
        /// Update parking slot (status, etc)
        /// </summary>
        [HttpPut("manager/{slotId}")]
        [ProducesResponseType(typeof(UpdateParkingSlotResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UpdateParkingSlotResultDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateForManager(string slotId, [FromBody] UpdateParkingSlotDto dto)
        {
            dto.SlotId = slotId;
            var result = await _parkingSlotService.UpdateParkingSlotAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // ── 3. Employee Slot Management ──

        /// <summary>
        /// Get slots list - Employee view (filtered, paginated)
        /// </summary>
        [HttpGet("employee/list")]
        [ProducesResponseType(typeof(ListEmployeeSlotDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetForEmployee([FromQuery] EmployeeSlotFilterDto filter)
        {
            var result = await _parkingSlotService.GetEmployeeSlotsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Get slot detail - Employee view
        /// </summary>
        [HttpGet("employee/{slotId}/detail")]
        [ProducesResponseType(typeof(EmployeeSlotDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetailForEmployee(string slotId)
        {
            try
            {
                var result = await _parkingSlotService.GetEmployeeSlotDetailAsync(slotId);
                return Ok(result);
            }
            catch
            {
                return NotFound(new { message = "Không tìm thấy chỗ đỗ" });
            }
        }
    }
}
