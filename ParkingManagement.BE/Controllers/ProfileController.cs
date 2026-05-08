using Microsoft.AspNetCore.Mvc;

namespace ParkingManagement.Web.Controllers
{
    public class ProfileController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();
    }
}
