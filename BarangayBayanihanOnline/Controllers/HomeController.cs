using Microsoft.AspNetCore.Mvc;

namespace BarangayBayanihanOnline.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Mission()
        {
            return View();
        }

        public IActionResult Team()
        {
            return View();
        }
        public IActionResult Media()
        {
            return View();
        }
    }
}