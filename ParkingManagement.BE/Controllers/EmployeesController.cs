using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.Web.Controllers.Api
{
    /// <summary>
    /// API for Employee Management
    /// Includes: Basic CRUD, Manager operations, and Invite processing
    /// </summary>
    [ApiController]
    [Route("api/employees")]
    [Produces("application/json")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // ── 1. Basic Employee Operations ──

        /// <summary>
        /// Get all employees
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<EmployeeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _employeeService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get employee by ID
        /// </summary>
        [HttpGet("{employeeId}")]
        [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(string employeeId)
        {
            var result = await _employeeService.GetByIdAsync(employeeId);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy nhân viên" });
            return Ok(result);
        }

        /// <summary>
        /// Search employees by keyword
        /// </summary>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<EmployeeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string keyword)
        {
            var result = await _employeeService.SearchAsync(keyword);
            return Ok(result);
        }

        /// <summary>
        /// Create new employee
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ProducesResponseType(typeof(ServiceResult<string>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ServiceResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeDto dto)
        {
            var result = await _employeeService.CreateAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return CreatedAtAction(nameof(GetById), new { employeeId = result.Data }, result);
        }

        /// <summary>
        /// Soft delete employee
        /// </summary>
        [HttpDelete("{employeeId}")]
        [ProducesResponseType(typeof(ServiceResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SoftDelete(string employeeId)
        {
            var result = await _employeeService.SoftDeleteAsync(employeeId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Get all deleted employees
        /// </summary>
        [HttpGet("deleted")]
        [ProducesResponseType(typeof(List<EmployeeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDeleted()
        {
            var result = await _employeeService.GetDeletedAsync();
            return Ok(result);
        }

        /// <summary>
        /// Restore deleted employee
        /// </summary>
        [HttpPatch("{employeeId}/restore")]
        [ProducesResponseType(typeof(ServiceResult<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Restore(string employeeId)
        {
            var result = await _employeeService.RestoreAsync(employeeId);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // ── 2. Manager Employee Management ──

        /// <summary>
        /// Get employee list (filtered, paginated) - Manager view
        /// </summary>
        [HttpGet("manager/list")]
        [ProducesResponseType(typeof(ListManagerEmployeeDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmployeesForManager([FromQuery] ManagerEmployeeFilterDto filter)
        {
            var result = await _employeeService.GetEmployeesAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Get employee detail with statistics - Manager view
        /// </summary>
        [HttpGet("manager/{employeeId}/detail")]
        [ProducesResponseType(typeof(ManagerEmployeeDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEmployeeDetailForManager(string employeeId)
        {
            try
            {
                var result = await _employeeService.GetEmployeeDetailAsync(employeeId);
                return Ok(result);
            }
            catch
            {
                return NotFound(new { message = "Không tìm thấy nhân viên" });
            }
        }

        /// <summary>
        /// Update employee information - Manager operation
        /// </summary>
        [HttpPut("manager/{employeeId}")]
        [ProducesResponseType(typeof(UpdateEmployeeResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(UpdateEmployeeResultDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateEmployee(string employeeId, [FromBody] UpdateEmployeeByManagerDto dto)
        {
            var result = await _employeeService.UpdateEmployeeAsync(employeeId, dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Delete employee - Manager operation
        /// </summary>
        [HttpPost("manager/delete")]
        [ProducesResponseType(typeof(DeleteEmployeeResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(DeleteEmployeeResultDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteEmployeeByManager([FromBody] DeleteEmployeeDto dto)
        {
            var result = await _employeeService.DeleteEmployeeAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // ── 3. Employee Invite Processing ──

        /// <summary>
        /// Create employee invite - Manager sends invite link
        /// </summary>
        [HttpPost("manager/invite")]
        [ProducesResponseType(typeof(CreateEmployeeInviteResultDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CreateEmployeeInviteResultDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateInvite([FromBody] CreateEmployeeInviteByManagerDto dto)
        {
            var result = await _employeeService.CreateEmployeeInviteAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return CreatedAtAction(nameof(CreateInvite), result);
        }

        /// <summary>
        /// Get invite details by token
        /// </summary>
        [HttpGet("invite/{token}")]
        [ProducesResponseType(typeof(ServiceResult<EmployeeInviteDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ServiceResult<EmployeeInviteDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetInviteByToken(string token)
        {
            var result = await _employeeService.GetInviteByTokenAsync(token);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        /// <summary>
        /// Confirm invite and activate account
        /// </summary>
        [HttpPost("invite/confirm")]
        [ProducesResponseType(typeof(ConfirmEmployeeInviteResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ConfirmEmployeeInviteResultDto), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ConfirmInvite([FromBody] ConfirmEmployeeInviteDto dto)
        {
            var result = await _employeeService.ConfirmInviteAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
    }
}
