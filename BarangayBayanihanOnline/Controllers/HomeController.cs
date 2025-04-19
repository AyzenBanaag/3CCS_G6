using BarangayBayanihanOnline.Models;
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
        public IActionResult OurProgram()
        {
            return View();
        }
        public IActionResult ContactUs()
        {
            return View();
        }
        public IActionResult MyEvents()
        {
            return View();
        }
        public IActionResult Volunteer()
        {
            return View();
        }
        public IActionResult Donate()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Submit(ContactForm model)
        {
            if (ModelState.IsValid)
            {
                // Here you would typically save to database
                // For now, we'll just pass the data to the view
                TempData["MessageSent"] = true;
                return View("Submit", model);
            }

            // If invalid, return to contact form
            return View("ContactUs", model);
        }

    }
}