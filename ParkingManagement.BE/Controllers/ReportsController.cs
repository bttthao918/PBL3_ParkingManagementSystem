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
    }
}
