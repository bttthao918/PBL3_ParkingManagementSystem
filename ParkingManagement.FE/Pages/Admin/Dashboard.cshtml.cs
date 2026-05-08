using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ParkingManagement.FE.Pages.Admin
{
    [Authorize(Roles = "Manager")]
    public class DashboardModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["Role"] = "Admin / Manager";
            ViewData["UserName"] = User.FindFirst(ClaimTypes.Name)?.Value ?? "Manager";
        }
    }
}
