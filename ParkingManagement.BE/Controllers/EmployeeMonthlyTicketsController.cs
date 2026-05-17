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
    /// API quản lý vé tháng dành cho Nhân viên
    /// Nhân viên có thể: xem danh sách, tạo mới, gia hạn, hủy vé tháng
    /// (Không có chỉnh sửa giá - chức năng đó chỉ dành cho Quản lý)
    /// </summary>
    [ApiController]
    [Route("api/employee/monthly-tickets")]
    [Authorize(Roles = "Employee")]
    [Produces("application/json")]
    public class EmployeeMonthlyTicketsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPricingService _pricingService;
        private readonly ILogger<EmployeeMonthlyTicketsController> _logger;

        public EmployeeMonthlyTicketsController(
            AppDbContext db,
            IPricingService pricingService,
            ILogger<EmployeeMonthlyTicketsController> logger)
        {
            _db = db;
            _pricingService = pricingService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách vé tháng với tìm kiếm và phân trang
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? status,
            [FromQuery] string? vehicleType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var query = _db.MonthlyTickets
                    .Include(t => t.Customer)
                    .AsQueryable();

                // Tìm kiếm theo biển số, tên khách, SĐT
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var keyword = search.Trim();
                    query = query.Where(t =>
                        t.VehiclePlate.Contains(keyword) ||
                        t.MonthlyTicketId.Contains(keyword) ||
                        (t.Customer != null && t.Customer.FullName.Contains(keyword)) ||
                        (t.Customer != null && t.Customer.PhoneNumber != null && t.Customer.PhoneNumber.Contains(keyword)));
                }

                // Lọc theo trạng thái
                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(t => t.Status == status);
                }

                // Lọc theo loại xe
                if (!string.IsNullOrWhiteSpace(vehicleType))
                {
                    query = query.Where(t => t.VehicleType == vehicleType);
                }

                var today = DateTime.Today;
                var total = await query.CountAsync();
                var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

                var tickets = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        t.MonthlyTicketId,
                        CustomerName = t.Customer != null ? t.Customer.FullName : "N/A",
                        CustomerPhone = t.Customer != null ? t.Customer.PhoneNumber : "N/A",
                        t.VehiclePlate,
                        t.VehicleType,
                        t.PackageType,
                        StartDate = t.StartDate,
                        EndDate = t.EndDate,
                        t.Status,
                        DaysRemaining = t.Status == "Hoạt động" ? Math.Max(0, (t.EndDate - today).Days) : 0,
                        t.TotalFee,
                        t.CreatedAt
                    })
                    .ToListAsync();

                // Thống kê tổng quan
                var allCount = await _db.MonthlyTickets.CountAsync();
                var activeCount = await _db.MonthlyTickets.CountAsync(t => t.Status == "Hoạt động");
                var expiredCount = await _db.MonthlyTickets.CountAsync(t => t.Status == "Hết hạn" || (t.Status == "Hoạt động" && t.EndDate < today));
                var expiringSoonCount = await _db.MonthlyTickets.CountAsync(t => t.Status == "Hoạt động" && t.EndDate >= today && t.EndDate <= today.AddDays(7));

                return Ok(new
                {
                    items = tickets,
                    totalItems = total,
                    totalPages,
                    page,
                    pageSize,
                    summary = new
                    {
                        total = allCount,
                        active = activeCount,
                        expired = expiredCount,
                        expiringSoon = expiringSoonCount
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetAll monthly tickets error");
                return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Lấy chi tiết vé tháng
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail(string id)
        {
            try
            {
                var ticket = await _db.MonthlyTickets
                    .Include(t => t.Customer)
                    .FirstOrDefaultAsync(t => t.MonthlyTicketId == id);

                if (ticket == null)
                    return NotFound(new { message = "Không tìm thấy vé tháng" });

                var payments = await _db.Payments
                    .Where(p => p.MonthlyTicketId == id)
                    .OrderByDescending(p => p.PaymentTime)
                    .ToListAsync();

                var today = DateTime.Today;

                return Ok(new
                {
                    ticket.MonthlyTicketId,
                    CustomerName = ticket.Customer?.FullName,
                    CustomerPhone = ticket.Customer?.PhoneNumber,
                    CustomerId = ticket.CustomerId,
                    ticket.VehiclePlate,
                    ticket.VehicleType,
                    ticket.PackageType,
                    StartDate = ticket.StartDate,
                    EndDate = ticket.EndDate,
                    ticket.Status,
                    DaysRemaining = ticket.Status == "Hoạt động" ? Math.Max(0, (ticket.EndDate - today).Days) : 0,
                    ticket.TotalFee,
                    CreatedAt = ticket.CreatedAt,
                    Payments = payments.Select(p => new
                    {
                        p.PaymentId,
                        p.Amount,
                        p.Method,
                        p.PaymentTime,
                        p.Status
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDetail monthly ticket error");
                return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Tạo vé tháng mới cho khách hàng
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeMonthlyTicketDto model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.VehiclePlate))
                    return BadRequest(new { success = false, message = "Biển số xe không được để trống" });

                if (string.IsNullOrWhiteSpace(model.VehicleType))
                    return BadRequest(new { success = false, message = "Loại xe không được để trống" });

                if (!IsSupportedDuration(model.DurationMonths))
                    return BadRequest(new { success = false, message = "Gói vé phải là 1, 3 hoặc 6 tháng" });

                // Kiểm tra xe đã có vé tháng đang hoạt động chưa
                var existing = await _db.MonthlyTickets
                    .FirstOrDefaultAsync(t => t.VehiclePlate == model.VehiclePlate && t.Status == "Hoạt động");
                if (existing != null)
                    return BadRequest(new { success = false, message = "Biển số xe này đã có vé tháng đang hoạt động" });

                // Tìm khách hàng
                Customer? customer = null;
                if (!string.IsNullOrWhiteSpace(model.CustomerId))
                {
                    customer = await _db.Customers.FindAsync(model.CustomerId);
                }
                else if (!string.IsNullOrWhiteSpace(model.CustomerPhone))
                {
                    customer = await _db.Customers.FirstOrDefaultAsync(c => c.PhoneNumber == model.CustomerPhone);
                }

                if (customer == null)
                    return BadRequest(new { success = false, message = "Không tìm thấy khách hàng. Vui lòng kiểm tra lại thông tin." });

                // Tạo/cập nhật vehicle
                var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.VehiclePlate == model.VehiclePlate);
                if (vehicle == null)
                {
                    vehicle = new Vehicle
                    {
                        VehiclePlate = model.VehiclePlate,
                        VehicleType = model.VehicleType,
                        CustomerId = customer.CustomerId
                    };
                    _db.Vehicles.Add(vehicle);
                    await _db.SaveChangesAsync();
                }

                // Tính phí
                decimal fee = await CalculateFeeAsync(model.VehicleType, model.DurationMonths);
                if (fee == 0)
                    return BadRequest(new { success = false, message = "Không tìm thấy bảng giá cho loại xe và gói này" });

                // Tạo vé tháng
                var startDate = DateTime.Today;
                var endDate = startDate.AddMonths(model.DurationMonths).AddDays(-1);
                var ticketId = GenerateId("MT");

                var monthlyTicket = new MonthlyTicket
                {
                    MonthlyTicketId = ticketId,
                    CustomerId = customer.CustomerId,
                    VehiclePlate = model.VehiclePlate,
                    VehicleType = model.VehicleType,
                    StartDate = startDate,
                    EndDate = endDate,
                    PackageType = model.DurationMonths + " tháng",
                    TotalFee = fee,
                    Status = "Hoạt động",
                    CreatedAt = DateTime.Now
                };

                _db.MonthlyTickets.Add(monthlyTicket);

                // Tạo payment record
                var paymentId = GenerateId("PAY");
                var payment = new Payment
                {
                    PaymentId = paymentId,
                    TicketId = null,
                    MonthlyTicketId = ticketId,
                    Amount = fee,
                    Method = PaymentMethods.Normalize(model.PaymentMethod),
                    PaymentTime = DateTime.Now,
                    Status = PaymentStatuses.SUCCESS,
                    CollectedByEmployeeId = GetEmployeeId()
                };
                _db.Payments.Add(payment);

                await _db.SaveChangesAsync();

                _logger.LogInformation("Employee created monthly ticket {TicketId} for plate {Plate}", ticketId, model.VehiclePlate);

                return Ok(new
                {
                    success = true,
                    message = "Tạo vé tháng thành công!",
                    data = new
                    {
                        monthlyTicketId = ticketId,
                        vehiclePlate = model.VehiclePlate,
                        vehicleType = model.VehicleType,
                        startDate,
                        endDate,
                        fee,
                        status = "Hoạt động"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create monthly ticket error");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        /// <summary>
        /// Gia hạn vé tháng
        /// </summary>
        [HttpPost("{id}/renew")]
        public async Task<IActionResult> Renew(string id, [FromBody] RenewEmployeeMonthlyTicketDto model)
        {
            try
            {
                if (!IsSupportedDuration(model.MonthsToAdd))
                    return BadRequest(new { success = false, message = "Gói gia hạn phải là 1, 3 hoặc 6 tháng" });

                var ticket = await _db.MonthlyTickets.FirstOrDefaultAsync(t => t.MonthlyTicketId == id);
                if (ticket == null)
                    return NotFound(new { success = false, message = "Không tìm thấy vé tháng" });

                // Tính phí gia hạn
                decimal renewFee = await CalculateFeeAsync(ticket.VehicleType, model.MonthsToAdd);
                if (renewFee == 0)
                    return BadRequest(new { success = false, message = "Không tìm thấy bảng giá cho gói gia hạn này" });

                // Gia hạn từ ngày hết hạn hiện tại (hoặc hôm nay nếu đã hết hạn)
                var baseDate = ticket.EndDate < DateTime.Today ? DateTime.Today : ticket.EndDate;
                var newEndDate = baseDate.AddMonths(model.MonthsToAdd);

                ticket.EndDate = newEndDate;
                ticket.Status = "Hoạt động";
                ticket.TotalFee += renewFee;

                // Tạo payment record
                var paymentId = GenerateId("PAY");
                var payment = new Payment
                {
                    PaymentId = paymentId,
                    TicketId = null,
                    MonthlyTicketId = id,
                    Amount = renewFee,
                    Method = PaymentMethods.Normalize(model.PaymentMethod),
                    PaymentTime = DateTime.Now,
                    Status = PaymentStatuses.SUCCESS,
                    CollectedByEmployeeId = GetEmployeeId()
                };
                _db.Payments.Add(payment);

                await _db.SaveChangesAsync();

                _logger.LogInformation("Employee renewed monthly ticket {TicketId} for {Months} months", id, model.MonthsToAdd);

                return Ok(new
                {
                    success = true,
                    message = $"Gia hạn {model.MonthsToAdd} tháng thành công!",
                    data = new
                    {
                        monthlyTicketId = id,
                        newEndDate,
                        renewFee,
                        totalFee = ticket.TotalFee
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Renew monthly ticket error");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Hủy vé tháng
        /// </summary>
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                var ticket = await _db.MonthlyTickets.FirstOrDefaultAsync(t => t.MonthlyTicketId == id);
                if (ticket == null)
                    return NotFound(new { success = false, message = "Không tìm thấy vé tháng" });

                if (ticket.Status == "Đã hủy")
                    return BadRequest(new { success = false, message = "Vé đã được hủy trước đó" });

                ticket.Status = "Đã hủy";
                await _db.SaveChangesAsync();

                _logger.LogInformation("Employee cancelled monthly ticket {TicketId}", id);

                return Ok(new
                {
                    success = true,
                    message = "Hủy vé tháng thành công!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cancel monthly ticket error");
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Lấy danh sách vé sắp hết hạn (trong 7 ngày)
        /// </summary>
        [HttpGet("expiring-soon")]
        public async Task<IActionResult> GetExpiringSoon([FromQuery] int days = 7)
        {
            try
            {
                var today = DateTime.Today;
                var tickets = await _db.MonthlyTickets
                    .Include(t => t.Customer)
                    .Where(t => t.Status == "Hoạt động" && t.EndDate >= today && t.EndDate <= today.AddDays(days))
                    .OrderBy(t => t.EndDate)
                    .Select(t => new
                    {
                        t.MonthlyTicketId,
                        CustomerName = t.Customer != null ? t.Customer.FullName : "N/A",
                        CustomerPhone = t.Customer != null ? t.Customer.PhoneNumber : "N/A",
                        t.VehiclePlate,
                        t.VehicleType,
                        EndDate = t.EndDate,
                        DaysRemaining = (t.EndDate - today).Days
                    })
                    .ToListAsync();

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetExpiringSoon error");
                return StatusCode(500, new { message = "Lỗi hệ thống" });
            }
        }

        /// <summary>
        /// Lấy bảng giá vé tháng
        /// </summary>
        [HttpGet("pricing")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPricing()
        {
            var currentPricing = await _pricingService.GetCurrentPricingAsync();
            var vehicleTypes = new[] { "Xe máy", "Ô tô nhỏ", "Ô tô lớn" };
            var months = new[] { 1, 3, 6 };

            var pricing = vehicleTypes
                .SelectMany(vehicleType => months.Select(month => new
                {
                    vehicleType,
                    months = month,
                    packageType = month + " tháng",
                    price = GetMonthlyPrice(currentPricing, vehicleType, month)
                }))
                .ToList();

            return Ok(pricing);
        }

        // ── Helpers ──

        private Task<decimal> CalculateFeeAsync(string vehicleType, int months)
        {
            return _pricingService.GetMonthlyTicketPriceAsync(vehicleType, months);
        }

        private static bool IsSupportedDuration(int months)
        {
            return months is 1 or 3 or 6;
        }

        private static decimal GetMonthlyPrice(PricingDto pricing, string vehicleType, int months)
        {
            if (!pricing.MonthlyTicketPrice.TryGetValue(vehicleType, out var monthlyPrice))
            {
                monthlyPrice = pricing.MonthlyTicketPrice
                    .FirstOrDefault(item => string.Equals(item.Key, vehicleType, StringComparison.OrdinalIgnoreCase))
                    .Value;
            }

            if (monthlyPrice == null)
            {
                return 0m;
            }

            return months switch
            {
                1 => monthlyPrice.OneMonth,
                3 => monthlyPrice.ThreeMonth,
                6 => monthlyPrice.SixMonth,
                _ => 0m
            };
        }

        private static string GenerateId(string prefix)
        {
            return prefix + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);
        }

        private string? GetEmployeeId()
        {
            return User.FindFirst("employeeId")?.Value
                ?? User.FindFirst("related_id")?.Value;
        }
    }

    // ── DTOs ──

    public class CreateEmployeeMonthlyTicketDto
    {
        public string VehiclePlate { get; set; } = null!;
        public string VehicleType { get; set; } = "Xe máy";
        public int DurationMonths { get; set; } = 1;
        public string? CustomerId { get; set; }
        public string? CustomerPhone { get; set; }
        public string? PaymentMethod { get; set; } = "Tiền mặt";
    }

    public class RenewEmployeeMonthlyTicketDto
    {
        public int MonthsToAdd { get; set; } = 1;
        public string? PaymentMethod { get; set; } = "Tiền mặt";
    }
}
