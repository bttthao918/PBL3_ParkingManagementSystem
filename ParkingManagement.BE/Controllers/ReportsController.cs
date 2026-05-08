using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.Web.Controllers.Api
{
    /// <summary>
    /// API for Reports & Statistics
    /// Includes: Basic reports, Manager dashboards, and Employee reports
    /// Access control should be enforced at the Controller/Action level
    /// </summary>
    [ApiController]
    [Route("api/reports")]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // ── 1. Basic Revenue Reports ──

        /// <summary>
        /// Get revenue report for date range
        /// </summary>
        [HttpGet("revenue")]
        [ProducesResponseType(typeof(RevenueReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var result = await _reportService.GetRevenueReportAsync(from, to);
            return Ok(result);
        }

        /// <summary>
        /// Get list of expiring monthly tickets
        /// </summary>
        [HttpGet("expiring-tickets")]
        [ProducesResponseType(typeof(List<MonthlyTicketDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExpiringTickets([FromQuery] int days = 7)
        {
            var result = await _reportService.GetExpiringSoonAsync(days);
            return Ok(result);
        }

        /// <summary>
        /// Count active vehicles currently parked
        /// </summary>
        [HttpGet("active-vehicles")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<IActionResult> CountActiveVehicles()
        {
            var result = await _reportService.CountActiveVehiclesAsync();
            return Ok(new { activeVehicles = result });
        }

        // ── 2. Manager Dashboard & Reports ──

        /// <summary>
        /// Get dashboard summary with KPIs - Manager view
        /// </summary>
        [HttpGet("manager/dashboard")]
        [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetManagerDashboard()
        {
            var result = await _reportService.GetDashboardSummaryAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get revenue report with filters - Manager view
        /// </summary>
        [HttpPost("manager/revenue")]
        [ProducesResponseType(typeof(RevenueReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetManagerRevenueReport([FromBody] RevenueReportFilterDto filter)
        {
            var result = await _reportService.GetRevenueReportAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Get customer report - Manager view
        /// </summary>
        [HttpGet("manager/customers")]
        [ProducesResponseType(typeof(CustomerReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetManagerCustomerReport()
        {
            var result = await _reportService.GetCustomerReportAsync();
            return Ok(result);
        }

        // ── 3. Employee Reports ──

        /// <summary>
        /// Get personal dashboard - Employee view
        /// </summary>
        [HttpGet("employee/{employeeId}/dashboard")]
        [ProducesResponseType(typeof(EmployeeDashboardDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployeeDashboard(string employeeId)
        {
            var result = await _reportService.GetEmployeeDashboardAsync(employeeId);
            return Ok(result);
        }

        /// <summary>
        /// Get shift attendance report - Employee view
        /// </summary>
        [HttpGet("employee/{employeeId}/attendance")]
        [ProducesResponseType(typeof(ShiftAttendanceReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetShiftAttendanceReport(
            string employeeId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _reportService.GetShiftAttendanceReportAsync(employeeId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Get personal revenue report - Employee view
        /// </summary>
        [HttpGet("employee/{employeeId}/revenue")]
        [ProducesResponseType(typeof(EmployeeRevenueReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmployeeRevenueReport(
            string employeeId,
            [FromQuery] string period = "month")
        {
            var result = await _reportService.GetEmployeeRevenueReportAsync(employeeId, period);
            return Ok(result);
        }
    }
}
