using Microsoft.AspNetCore.Mvc;

namespace ParkingManagement.Web.Controllers
{
    public class ParkingLotController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();
    }
}
