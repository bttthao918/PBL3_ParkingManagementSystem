using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Customer
{
    [Authorize(Roles = "Customer")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["Role"] = "Khách hàng";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Customer";
        }
    }
}
