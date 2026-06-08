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
        /// Get revenue report for manager/admin statistics page.
        /// </summary>
        [HttpPost("manager/revenue")]
        [ProducesResponseType(typeof(RevenueReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetManagerRevenueReport([FromBody] RevenueReportFilterDto filter)
        {
            var result = await _reportService.GetRevenueReportAsync(filter ?? new RevenueReportFilterDto());
            return Ok(result);
        }

        /// <summary>
        /// Get customer statistics report for manager/admin statistics page.
        /// </summary>
        [HttpGet("manager/customers")]
        [ProducesResponseType(typeof(CustomerReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetManagerCustomerReport(
            [FromQuery] string period = "30days",
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _reportService.GetCustomerReportAsync(period, fromDate, toDate);
            return Ok(result);
        }

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
        /// Get attendance report for employee statistics pages.
        /// </summary>
        [HttpGet("employee/{employeeId}/attendance")]
        [ProducesResponseType(typeof(ShiftAttendanceReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmployeeAttendanceReport(
            string employeeId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _reportService.GetShiftAttendanceReportAsync(employeeId, fromDate, toDate);
            return Ok(result);
        }

        /// <summary>
        /// Get revenue report for employee statistics pages.
        /// </summary>
        [HttpGet("employee/{employeeId}/revenue")]
        [ProducesResponseType(typeof(EmployeeRevenueReportDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmployeeRevenueReport(
            string employeeId,
            [FromQuery] string period = "month",
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _reportService.GetEmployeeRevenueReportAsync(employeeId, period, fromDate, toDate);
            return Ok(result);
        }
    }
}
