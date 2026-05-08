using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Employee
{
    [Authorize(Roles = "Employee")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["Role"] = "Nhân viên";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Employee";
        }
    }
}
