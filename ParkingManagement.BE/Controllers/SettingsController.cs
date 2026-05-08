using Microsoft.AspNetCore.Mvc;

namespace ParkingManagement.Web.Controllers
{
    public class SettingsController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();
    }
}
