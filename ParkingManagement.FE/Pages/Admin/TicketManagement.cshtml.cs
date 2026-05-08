using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Admin
{
        [Authorize(Roles = "Manager")]
    public class TicketManagementModel : PageModel
        {
            public List<TicketViewModel> Tickets { get; set; } = new();

            public void OnGet()
            {
                Tickets = new List<TicketViewModel>
            {
                new("VE0002345", "30A-123.45", "Nguyễn Văn An", "Vé tháng", 1200000, "Đang hoạt động", "active"),
                new("VE0002344", "30A-678.90", "Trần Thị Hoa", "Vé ngày", 50000, "Đang hoạt động", "active"),
                new("VE0002343", "30A-111.22", "Lê Văn Cường", "Vé tháng", 1200000, "Đang hoạt động", "active"),
                new("VE0002342", "30A-333.44", "Phạm Minh Tuấn", "Vé ngày", 50000, "Hết hạn", "expired"),
                new("VE0002341", "30A-555.66", "Hoàng Thị Mai", "Vé tháng", 1200000, "Hết hạn", "expired"),
                new("VE0002340", "30A-777.88", "Đỗ Văn Hải", "Vé ngày", 50000, "Đã hủy", "cancelled"),
                new("VE0002339", "30A-999.00", "Bùi Thị Lan", "Vé tháng", 1200000, "Đang hoạt động", "active")
            };
            }
        }

        [Authorize(Roles = "Manager")]
    public class TicketViewModel
        {
            public string Code { get; set; }
            public string PlateNumber { get; set; }
            public string CustomerName { get; set; }
            public string Type { get; set; }
            public decimal Price { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; }
            public string StatusClass { get; set; }

            public TicketViewModel(
                string code,
                string plateNumber,
                string customerName,
                string type,
                decimal price,
                string status,
                string statusClass)
            {
                Code = code;
                PlateNumber = plateNumber;
                CustomerName = customerName;
                Type = type;
                Price = price;
                Status = status;
                StatusClass = statusClass;
                StartDate = DateTime.Now.AddDays(-5);
                EndDate = DateTime.Now.AddDays(25);
            }
        }
}
