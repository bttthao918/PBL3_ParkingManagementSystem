using Microsoft.AspNetCore.Mvc;

namespace ParkingManagement.Web.Controllers
{
    public class DashboardController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();
    }
}
