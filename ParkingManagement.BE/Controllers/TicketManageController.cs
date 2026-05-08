using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParkingManagement.DAL.Data;
using ParkingManagement.DAL.Models;

namespace ParkingManagement.Web.Controllers
{
    [Area("admin")]
    [Route("admin/ticket")]
    public class TicketManageController : Controller
    {
        private readonly AppDbContext _db;

        public TicketManageController(AppDbContext db)
        {
            _db = db;
        }

        // ==================== TRANG CHÍNH ====================
        [HttpGet]
        [Route("")]
        [Route("index")]
        public IActionResult Index()
        {
            return View();
        }

        // ==================== DASHBOARD API ====================
        [HttpGet]
        [Route("get-stats")]
        public async Task<IActionResult> GetStats()
        {
            var today = DateTime.Today;

            var stats = new
            {
                totalLaneTickets = await _db.Tickets.CountAsync(),
                usedLaneTickets = await _db.Tickets.CountAsync(t => t.Status == "Đã ra"),
                totalMonthlyTickets = await _db.MonthlyTickets.CountAsync(),
                activeMonthlyTickets = await _db.MonthlyTickets.CountAsync(t => t.Status == "Hoạt động" && t.EndDate >= today),
                expiringSoon = await _db.MonthlyTickets.CountAsync(t => t.EndDate >= today && t.EndDate <= today.AddDays(7) && t.Status == "Hoạt động"),
                todayRevenue = await _db.Payments.Where(p => p.PaymentTime.Date == today && p.Status == "Thành công").SumAsync(p => p.Amount),
                totalRevenue = await _db.Payments.Where(p => p.Status == "Thành công").SumAsync(p => p.Amount)
            };

            return Json(stats);
        }

        // ==================== VÉ LƯỢT ====================

        [HttpGet]
        [Route("lane/list")]
        public async Task<IActionResult> GetLaneTickets(string search = "", string status = "", int page = 1, int pageSize = 10)
        {
            var query = _db.Tickets.Include(t => t.Vehicle).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.TicketId.Contains(search) ||
                                         t.VehiclePlate.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "used") query = query.Where(t => t.Status == "Đã ra");
                else if (status == "unused") query = query.Where(t => t.Status == "Đang trong bãi");
            }

            var total = await query.CountAsync();
            var tickets = await query
                .OrderByDescending(t => t.CheckInTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    LaneTicketId = t.TicketId,
                    TicketCode = t.TicketId,
                    LicensePlate = t.VehiclePlate,
                    VehicleType = t.VehicleType,
                    IssuedAt = t.CheckInTime.ToString("dd/MM/yyyy HH:mm"),
                    IsUsed = t.Status == "Đã ra",
                    Price = t.Fee
                })
                .ToListAsync();

            return Json(new { data = tickets, total = total, page = page, pageSize = pageSize });
        }

        [HttpGet]
        [Route("lane/detail/{id}")]
        public async Task<IActionResult> GetLaneTicketDetail(string id)
        {
            var ticket = await _db.Tickets
                .Include(t => t.Vehicle)
                .Include(t => t.ParkingSlot)
                .FirstOrDefaultAsync(t => t.TicketId == id);

            if (ticket == null)
                return Json(new { success = false, message = "Không tìm thấy vé!" });

            return Json(new
            {
                LaneTicketId = ticket.TicketId,
                TicketCode = ticket.TicketId,
                LicensePlate = ticket.VehiclePlate,
                VehicleType = ticket.VehicleType,
                IssuedAt = ticket.CheckInTime.ToString("dd/MM/yyyy HH:mm"),
                IsUsed = ticket.Status == "Đã ra",
                Status = ticket.Status,
                Sessions = new[] { new
                {
                    SessionId = ticket.TicketId,
                    SpotNumber = ticket.ParkingSlot?.Location ?? "N/A",
                    CheckinTime = ticket.CheckInTime.ToString("dd/MM/yyyy HH:mm"),
                    CheckoutTime = ticket.CheckOutTime?.ToString("dd/MM/yyyy HH:mm"),
                    TotalAmount = ticket.Fee
                }}
            });
        }

        [HttpPost]
        [Route("lane/create")]
        public async Task<IActionResult> CreateLaneTicket([FromBody] CreateLaneTicketModel model)
        {
            try
            {
                var vehicle = await _db.Vehicles
                    .FirstOrDefaultAsync(v => v.VehiclePlate == model.LicensePlate);

                if (vehicle == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy xe với biển số này!" });
                }

                var ticket = new Ticket
                {
                    TicketId = GenerateTicketCode("L"),
                    VehiclePlate = vehicle.VehiclePlate,
                    VehicleType = vehicle.VehicleType,
                    CheckInTime = DateTime.Now,
                    Status = "Đang trong bãi",
                    Fee = 0
                };

                _db.Tickets.Add(ticket);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Tạo vé lượt thành công!", ticketCode = ticket.TicketId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("lane/mark-used/{id}")]
        public async Task<IActionResult> MarkLaneTicketAsUsed(string id)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vé!" });
            }

            if (ticket.Status == "Đã ra")
            {
                return Json(new { success = false, message = "Vé đã được sử dụng trước đó!" });
            }

            ticket.Status = "Đã ra";
            ticket.CheckOutTime = DateTime.Now;
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Đã đánh dấu vé đã sử dụng!" });
        }

        [HttpPost]
        [Route("lane/cancel/{id}")]
        public async Task<IActionResult> CancelLaneTicket(string id)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vé!" });
            }

            if (ticket.Status == "Đã ra")
            {
                return Json(new { success = false, message = "Vé đã được sử dụng, không thể hủy!" });
            }
            
            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Đã hủy vé thành công!" });
        }

        [HttpDelete]
        [Route("lane/delete/{id}")]
        public async Task<IActionResult> DeleteLaneTicket(string id)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vé!" });
            }

            _db.Tickets.Remove(ticket);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa vé thành công!" });
        }

        // ==================== VÉ THÁNG ====================

        [HttpGet]
        [Route("monthly/list")]
        public async Task<IActionResult> GetMonthlyTickets(string search = "", string status = "", int page = 1, int pageSize = 10)
        {
            var query = _db.MonthlyTickets
                .Include(t => t.Customer)
                    .ThenInclude(c => c.Account)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.VehiclePlate.Contains(search) ||
                                          (t.Customer != null && t.Customer.FullName.Contains(search)) ||
                                          (t.Customer != null && t.Customer.PhoneNumber.Contains(search)));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(t => t.Status == status);
            }

            var total = await query.CountAsync();
            var today = DateTime.Today;

            var tickets = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    MonthlyTicketId = t.MonthlyTicketId,
                    CustomerName = t.Customer != null ? t.Customer.FullName : "N/A",
                    CustomerPhone = t.Customer != null ? t.Customer.PhoneNumber : "N/A",
                    LicensePlate = t.VehiclePlate,
                    VehicleType = t.VehicleType,
                    StartDate = t.StartDate.ToString("dd/MM/yyyy"),
                    EndDate = t.EndDate.ToString("dd/MM/yyyy"),
                    Status = t.Status,
                    DaysRemaining = (t.EndDate - today).Days,
                    Price = t.TotalFee
                })
                .ToListAsync();

            return Json(new { data = tickets, total = total, page = page, pageSize = pageSize });
        }

        [HttpGet]
        [Route("monthly/detail/{id}")]
        public async Task<IActionResult> GetMonthlyTicketDetail(string id)
        {
            var ticket = await _db.MonthlyTickets
                .Include(t => t.Customer)
                    .ThenInclude(c => c.Account)
                .FirstOrDefaultAsync(t => t.MonthlyTicketId == id);

            if (ticket == null)
                return Json(new { success = false, message = "Không tìm thấy vé!" });

            var payments = await _db.Payments.Where(p => p.MonthlyTicketId == id).ToListAsync();

            return Json(new
            {
                MonthlyTicketId = ticket.MonthlyTicketId,
                CustomerName = ticket.Customer?.FullName,
                CustomerPhone = ticket.Customer?.PhoneNumber,
                CustomerEmail = ticket.Customer?.Account?.Email,
                LicensePlate = ticket.VehiclePlate,
                VehicleType = ticket.VehicleType,
                StartDate = ticket.StartDate.ToString("dd/MM/yyyy"),
                EndDate = ticket.EndDate.ToString("dd/MM/yyyy"),
                Status = ticket.Status,
                CreatedAt = ticket.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                PricePlan = new
                {
                    PackageType = ticket.PackageType,
                    TotalFee = ticket.TotalFee
                },
                Payments = payments.Select(p => new
                {
                    p.Amount,
                    p.Method,
                    PaymentTime = p.PaymentTime.ToString("dd/MM/yyyy HH:mm"),
                    p.Status
                })
            });
        }

        [HttpPost]
        [Route("monthly/create")]
        public async Task<IActionResult> CreateMonthlyTicket([FromBody] CreateMonthlyTicketModel model)
        {
            try
            {
                var customer = await _db.Customers.Include(c => c.Account)
                    .FirstOrDefaultAsync(c => c.PhoneNumber == model.Phone || (c.Account != null && c.Account.Email == model.Email));

                if (customer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khách hàng. Vui lòng đăng ký tài khoản trước." });
                }

                var vehicle = await _db.Vehicles
                    .FirstOrDefaultAsync(v => v.VehiclePlate == model.LicensePlate);

                if (vehicle == null)
                {
                    vehicle = new Vehicle
                    {
                        VehiclePlate = model.LicensePlate,
                        VehicleType = model.VehicleType,
                        CustomerId = customer.CustomerId
                    };
                    _db.Vehicles.Add(vehicle);
                    await _db.SaveChangesAsync();
                }

                var startDate = DateTime.Now.Date;
                var endDate = startDate.AddMonths(model.DurationMonths);

                var ticket = new MonthlyTicket
                {
                    MonthlyTicketId = GenerateTicketCode("M"),
                    VehiclePlate = vehicle.VehiclePlate,
                    VehicleType = vehicle.VehicleType,
                    CustomerId = customer.CustomerId,
                    StartDate = startDate,
                    EndDate = endDate,
                    PackageType = model.DurationMonths + " tháng",
                    TotalFee = model.DurationMonths * 300000, 
                    Status = "Hoạt động",
                    CreatedAt = DateTime.Now
                };

                _db.MonthlyTickets.Add(ticket);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Tạo vé tháng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPut]
        [Route("monthly/update/{id}")]
        public async Task<IActionResult> UpdateMonthlyTicket(string id, [FromBody] UpdateMonthlyTicketModel model)
        {
            try
            {
                var ticket = await _db.MonthlyTickets.FindAsync(id);
                if (ticket == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy vé!" });
                }

                ticket.Status = model.Status;

                if (model.NewEndDate.HasValue)
                {
                    ticket.EndDate = model.NewEndDate.Value;
                }

                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật vé tháng thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("monthly/renew/{id}")]
        public async Task<IActionResult> RenewMonthlyTicket(string id, [FromBody] RenewTicketModel model)
        {
            try
            {
                var ticket = await _db.MonthlyTickets
                    .FirstOrDefaultAsync(t => t.MonthlyTicketId == id);

                if (ticket == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy vé!" });
                }

                var newEndDate = ticket.EndDate.AddMonths(model.MonthsToAdd);
                ticket.EndDate = newEndDate;
                ticket.Status = "Hoạt động";
                
                decimal renewAmount = model.MonthsToAdd * 300000;
                ticket.TotalFee += renewAmount;

                await _db.SaveChangesAsync();

                return Json(new { success = true, message = $"Gia hạn {model.MonthsToAdd} tháng thành công!", amount = renewAmount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Route("monthly/cancel/{id}")]
        public async Task<IActionResult> CancelMonthlyTicket(string id)
        {
            try
            {
                var ticket = await _db.MonthlyTickets
                    .FirstOrDefaultAsync(t => t.MonthlyTicketId == id);

                if (ticket == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy vé!" });
                }

                if (ticket.Status == "Đã hủy")
                {
                    return Json(new { success = false, message = "Vé đã được hủy trước đó!" });
                }

                var today = DateTime.Today;
                var daysUsed = (today - ticket.StartDate).Days;
                var totalDays = (ticket.EndDate - ticket.StartDate).Days;

                decimal refundPercent = 0;
                if (totalDays > 0 && daysUsed < totalDays)
                {
                    refundPercent = (decimal)(totalDays - daysUsed) / totalDays * 100;
                }

                ticket.Status = "Đã hủy";
                await _db.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Đã hủy vé thành công! Hoàn trả khoảng {refundPercent:F0}% giá trị vé.",
                    refundPercent = refundPercent
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        [Route("monthly/delete/{id}")]
        public async Task<IActionResult> DeleteMonthlyTicket(string id)
        {
            try
            {
                var ticket = await _db.MonthlyTickets.FindAsync(id);
                if (ticket == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy vé!" });
                }

                _db.MonthlyTickets.Remove(ticket);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa vé thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==================== PHIÊN GỬI XE ĐANG HOẠT ĐỘNG ====================

        [HttpGet]
        [Route("active-sessions")]
        public async Task<IActionResult> GetActiveSessions()
        {
            var sessions = await _db.Tickets
                .Include(s => s.Vehicle)
                .Include(s => s.ParkingSlot)
                .Where(s => s.Status == "Đang trong bãi")
                .Select(s => new
                {
                    SessionId = s.TicketId,
                    LicensePlate = s.VehiclePlate,
                    SpotNumber = s.ParkingSlot != null ? s.ParkingSlot.Location : "N/A",
                    CheckinTime = s.CheckInTime.ToString("dd/MM/yyyy HH:mm"),
                    Duration = (DateTime.Now - s.CheckInTime).TotalHours.ToString("F1") + " giờ"
                })
                .ToListAsync();

            return Json(sessions);
        }

        // ==================== VÉ SẮP HẾT HẠN ====================

        [HttpGet]
        [Route("expiring-soon")]
        public async Task<IActionResult> GetExpiringSoonTickets()
        {
            var today = DateTime.Today;
            var expiringTickets = await _db.MonthlyTickets
                .Include(t => t.Customer)
                    .ThenInclude(c => c.Account)
                .Where(t => t.Status == "Hoạt động" && t.EndDate >= today && t.EndDate <= today.AddDays(7))
                .Select(t => new
                {
                    MonthlyTicketId = t.MonthlyTicketId,
                    CustomerName = t.Customer != null ? t.Customer.FullName : "N/A",
                    CustomerPhone = t.Customer != null ? t.Customer.PhoneNumber : "N/A",
                    LicensePlate = t.VehiclePlate,
                    EndDate = t.EndDate.ToString("dd/MM/yyyy"),
                    DaysRemaining = (t.EndDate - today).Days
                })
                .OrderBy(t => t.DaysRemaining)
                .ToListAsync();

            return Json(expiringTickets);
        }

        // ==================== HELPER ====================

        private string GenerateTicketCode(string prefix)
        {
            return prefix + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(100, 999);
        }
    }

    // Models
    public class CreateLaneTicketModel
    {
        public string LicensePlate { get; set; }
    }

    public class CreateMonthlyTicketModel
    {
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string LicensePlate { get; set; }
        public string VehicleType { get; set; }
        public int DurationMonths { get; set; } = 1;
    }

    public class UpdateMonthlyTicketModel
    {
        public string Status { get; set; }
        public DateTime? NewEndDate { get; set; }
    }

    public class RenewTicketModel
    {
        public int MonthsToAdd { get; set; }
    }
}
