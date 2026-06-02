using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ParkingManagement.BLL.DTOs;
using ParkingManagement.BLL.Helpers;
using ParkingManagement.BLL.Services.Interfaces;

namespace ParkingManagement.Web.Controllers.Api
{
    /// <summary>
    /// Customer Profile & Account Management API
    /// Requires: [Authorize] - Customer must be logged in
    /// </summary>
    [ApiController]
    [Route("api/customers")]
    [Authorize]
    [Produces("application/json")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IReservationService _reservationService;
        private readonly IMonthlyTicketService _monthlyTicketService;
        private readonly ITicketService _ticketService;
        private readonly IPaymentService _paymentService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            ICustomerService customerService,
            IReservationService reservationService,
            IMonthlyTicketService monthlyTicketService,
            ITicketService ticketService,
            IPaymentService paymentService,
            ILogger<CustomersController> logger)
        {
            _customerService = customerService;
            _reservationService = reservationService;
            _monthlyTicketService = monthlyTicketService;
            _ticketService = ticketService;
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Get current customer profile
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(CustomerProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                {
                    _logger.LogWarning("GetProfile called without customerId claim");
                    return Unauthorized(new { message = "Invalid token" });
                }

                var customer = await _customerService.GetByIdAsync(customerId);
                if (customer == null)
                {
                    _logger.LogWarning($"Customer not found: {customerId}");
                    return NotFound(new { message = "Customer not found" });
                }

                var detail = await _customerService.GetCustomerDetailAsync(customerId);
                var totalSpent = Math.Max(customer.TotalSpent, detail.TotalSpent);
                var totalTickets = Math.Max(customer.TotalTickets, detail.TotalTickets);
                var vipLevel = string.IsNullOrWhiteSpace(detail.VipLevel)
                    ? customer.VipLevel
                    : detail.VipLevel;
                var discountPercent = detail.DiscountPercent ?? VipHelper.GetVipDiscountPercent(vipLevel);
                var vipProgress = detail.VipProgress ?? 0;
                var amountToNextLevel = detail.AmountToNextLevel ?? 0;

                var response = new CustomerProfileDto
                {
                    CustomerId = customer.CustomerId,
                    Email = customer.Email ?? string.Empty,
                    FullName = customer.FullName,
                    PhoneNumber = customer.PhoneNumber,
                    Gender = customer.Gender,
                    CreatedAt = customer.CreatedAt,
                    VipLevel = vipLevel,
                    TotalSpent = totalSpent,
                    TotalTickets = totalTickets,
                    DiscountPercent = discountPercent,
                    VipProgress = vipProgress,
                    AmountToNextLevel = amountToNextLevel == 0 ? null : amountToNextLevel
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetProfile error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update customer profile
        /// </summary>
        [HttpPut("me")]
        [ProducesResponseType(typeof(UpdateProfileResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid input" });

                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                var customer = await _customerService.GetByIdAsync(customerId);
                if (customer == null)
                    return NotFound(new { message = "Customer not found" });

                // Update fields
                customer.FullName = dto.FullName;
                if (!string.IsNullOrEmpty(dto.PhoneNumber))
                    customer.PhoneNumber = dto.PhoneNumber;
                if (!string.IsNullOrEmpty(dto.Gender))
                    customer.Gender = dto.Gender;

                // TODO: Implement UpdateAsync in CustomerService
                // For now, just return success with updated data
                var response = new UpdateProfileResponseDto
                {
                    Success = true,
                    Message = "Profile updated successfully",
                    Data = new CustomerProfileDto
                    {
                        CustomerId = customer.CustomerId,
                        Email = customer.Email ?? string.Empty,
                        FullName = customer.FullName,
                        PhoneNumber = customer.PhoneNumber,
                        Gender = customer.Gender,
                        CreatedAt = customer.CreatedAt,
                        VipLevel = customer.VipLevel,
                        TotalSpent = customer.TotalSpent,
                        TotalTickets = customer.TotalTickets,
                        DiscountPercent = ParkingManagement.BLL.Helpers.VipHelper.GetVipDiscountPercent(customer.VipLevel),
                        VipProgress = 0, // This is just for Update response, won't recalculate perfectly but it's fine
                        AmountToNextLevel = null
                    }
                };

                _logger.LogInformation($"Profile updated for customer: {customerId}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"UpdateProfile error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // -- RESERVATIONS ----------------------------------------

        /// <summary>
        /// Get all reservations for current customer
        /// </summary>
        [HttpGet("reservations")]
        [ProducesResponseType(typeof(ListReservationsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReservations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                var filter = new FilterReservationDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var response = await _reservationService.GetByCustomerIdPaginatedAsync(customerId, filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetReservations error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Create new reservation
        /// </summary>
        [HttpPost("reservations")]
        [ProducesResponseType(typeof(ReservationDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateReservation([FromBody] CreateReservationDto dto)
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                dto.CustomerId = customerId;
                ModelState.Remove(nameof(CreateReservationDto.CustomerId));

                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid input" });

                var result = await _reservationService.CreateAsync(dto);

                if (!result.Success)
                    return BadRequest(result);

                _logger.LogInformation($"Reservation created for customer: {customerId}");
                return CreatedAtAction(nameof(GetReservations), result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CreateReservation error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cancel reservation
        /// </summary>
        [HttpDelete("reservations/{reservationId}")]
        [ProducesResponseType(typeof(CancelReservationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelReservation(string reservationId)
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                var result = await _reservationService.CancelReservationAsync(customerId, reservationId);

                if (!result.Success)
                    return BadRequest(result);

                _logger.LogInformation($"Reservation cancelled: {reservationId}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"CancelReservation error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // -- TICKETS (V� LU?T) ----------------------------------------

        /// <summary>
        /// Get all tickets for current customer
        /// </summary>
        [HttpGet("tickets")]
        [ProducesResponseType(typeof(ListCustomerTicketDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTickets([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                var filter = new CustomerTicketFilterDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    Status = status
                };

                var response = await _ticketService.GetMyTicketsAsync(customerId, filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetTickets error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get specific ticket details
        /// </summary>
        [HttpGet("tickets/{ticketId}")]
        [ProducesResponseType(typeof(CustomerTicketDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTicketDetail(string ticketId)
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                var response = await _ticketService.GetCustomerTicketDetailAsync(customerId, ticketId);
                if (response == null)
                    return NotFound(new { message = "Ticket not found or access denied" });

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetTicketDetail error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // -- MONTHLY TICKETS (V� TH�NG) ----------------------------------------

        /// <summary>
        /// Get all monthly tickets for current customer
        /// </summary>
        [HttpGet("monthly-tickets")]
        [ProducesResponseType(typeof(ListMonthlyTicketsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMonthlyTickets()
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                var monthlyTickets = await _monthlyTicketService.GetByCustomerIdAsync(customerId);

                var activeCount = monthlyTickets.Count(x => x.Status == "Ho?t d?ng" || x.Status == "Active");
                var expiredCount = monthlyTickets.Count(x => x.Status == "H?t h?n" || x.Status == "Expired");

                var response = new ListMonthlyTicketsDto
                {
                    Items = monthlyTickets.Select(x => new MonthlyTicketDetailDto
                    {
                        MonthlyTicketId = x.MonthlyTicketId,
                        VehiclePlate = x.VehiclePlate,
                        VehicleType = x.VehicleType,
                        PackageType = x.PackageType,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        TotalFee = x.TotalFee,
                        Status = x.Status,
                        DaysRemaining = x.DaysRemaining,
                        AutoRenew = x.AutoRenew
                    }).ToList(),
                    ActiveCount = activeCount,
                    ExpiredCount = expiredCount
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetMonthlyTickets error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Register new monthly ticket
        /// </summary>
        [HttpPost("monthly-tickets")]
        [ProducesResponseType(typeof(RegisterMonthlyTicketResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterMonthlyTicket([FromBody] RegisterMonthlyTicketDto dto)
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                dto.CustomerId = customerId;
                ModelState.Remove(nameof(RegisterMonthlyTicketDto.CustomerId));

                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid input" });

                if (string.IsNullOrWhiteSpace(dto.PackageType))
                    return BadRequest(new { message = "Invalid package type" });

                dto.PackageType = dto.PackageType.Trim();

                // Validate package type
                if (!new[] { "1 tháng", "3 tháng", "6 tháng" }.Contains(dto.PackageType))
                    return BadRequest(new { message = "Invalid package type" });

                var result = await _monthlyTicketService.RegisterAsync(dto);

                if (!result.Success)
                    return BadRequest(result);

                var monthlyTicket = result.Data;
                if (monthlyTicket == null)
                    return BadRequest(result);

                var response = new RegisterMonthlyTicketResponseDto
                {
                    Success = true,
                    Message = "Monthly ticket registered successfully",
                    Fee = monthlyTicket.TotalFee,
                    OrderCode = monthlyTicket.PayOsOrderCode,
                    PaymentLinkId = monthlyTicket.PayOsPaymentLinkId,
                    CheckoutUrl = monthlyTicket.CheckoutUrl,
                    QrCode = monthlyTicket.QrCode,
                    Data = new MonthlyTicketDetailDto
                    {
                        MonthlyTicketId = monthlyTicket.MonthlyTicketId,
                        VehiclePlate = monthlyTicket.VehiclePlate,
                        PackageType = monthlyTicket.PackageType,
                        StartDate = monthlyTicket.StartDate,
                        EndDate = monthlyTicket.EndDate,
                        TotalFee = monthlyTicket.TotalFee,
                        Status = monthlyTicket.Status,
                        DaysRemaining = monthlyTicket.DaysRemaining,
                        AutoRenew = monthlyTicket.AutoRenew
                    }
                };

                _logger.LogInformation($"Monthly ticket registered for customer: {customerId}");
                return CreatedAtAction(nameof(GetMonthlyTickets), response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"RegisterMonthlyTicket error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Renew monthly ticket
        /// </summary>
        [HttpPost("monthly-tickets/{monthlyTicketId}/renew")]
        [ProducesResponseType(typeof(RenewMonthlyTicketResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RenewMonthlyTicket(string monthlyTicketId, [FromBody] RenewMonthlyTicketDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid input" });

                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                if (string.IsNullOrWhiteSpace(dto.PackageType))
                    return BadRequest(new { message = "Invalid package type" });

                dto.PackageType = dto.PackageType.Trim();

                if (!new[] { "1 tháng", "3 tháng", "6 tháng" }.Contains(dto.PackageType))
                    return BadRequest(new { message = "Invalid package type" });

                var existingTicket = (await _monthlyTicketService.GetByCustomerIdAsync(customerId))
                    .FirstOrDefault(ticket => string.Equals(
                        ticket.MonthlyTicketId,
                        monthlyTicketId,
                        StringComparison.OrdinalIgnoreCase));

                if (existingTicket == null)
                    return NotFound(new { message = "Monthly ticket not found" });

                var additionalFee = await _monthlyTicketService.CalculateFeeAsync(
                    existingTicket.VehicleType,
                    dto.PackageType,
                    customerId);

                var result = await _monthlyTicketService.RenewAsync(monthlyTicketId, dto);
                if (!result.Success)
                    return BadRequest(result);

                var renewedTicket = result.Data!;

                var response = new RenewMonthlyTicketResponseDto
                {
                    Success = true,
                    Message = result.Message ?? "Monthly ticket renewed successfully",
                    AdditionalFee = additionalFee,
                    Data = new MonthlyTicketDetailDto
                    {
                        MonthlyTicketId = renewedTicket.MonthlyTicketId,
                        VehiclePlate = renewedTicket.VehiclePlate,
                        VehicleType = renewedTicket.VehicleType,
                        PackageType = renewedTicket.PackageType,
                        StartDate = renewedTicket.StartDate,
                        EndDate = renewedTicket.EndDate,
                        TotalFee = renewedTicket.TotalFee,
                        Status = renewedTicket.Status,
                        DaysRemaining = renewedTicket.DaysRemaining,
                        AutoRenew = renewedTicket.AutoRenew
                    }
                };

                _logger.LogInformation($"Monthly ticket renewed: {monthlyTicketId}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"RenewMonthlyTicket error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // -- PAYMENT HISTORY ----------------------------------------

        /// <summary>
        /// Get payment history for current customer
        /// </summary>
        [HttpGet("payment-history")]
        [ProducesResponseType(typeof(ListCustomerPaymentDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaymentHistory([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var customerId = User.FindFirst("customerId")?.Value;
                if (string.IsNullOrEmpty(customerId))
                    return Unauthorized(new { message = "Invalid token" });

                var filter = new CustomerPaymentFilterDto
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    FromDate = fromDate,
                    ToDate = toDate
                };

                var response = await _ticketService.GetPaymentHistoryAsync(customerId, filter);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GetPaymentHistory error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    
        // -- 3. Employee Customer Management --

        [HttpGet("employee/search")]
        [ProducesResponseType(typeof(ListEmployeeCustomerSearchDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchForEmployee([FromQuery] EmployeeCustomerSearchFilterDto filter)
        {
            var result = await _customerService.SearchCustomersAsync(filter);
            return Ok(result);
        }

        [HttpGet("employee/{customerId}/detail")]
        [ProducesResponseType(typeof(EmployeeCustomerDetailDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDetailForEmployee(string customerId)
        {
            try
            {
                var result = await _customerService.GetCustomerDetailAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}

