using Microsoft.AspNetCore.Mvc;

namespace BarangayBayanihanOnline.Controllers
{
    public class EventController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
