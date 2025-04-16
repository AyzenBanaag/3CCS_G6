using BarangayBayanihanOnline.Models;
using Microsoft.AspNetCore.Mvc;

namespace BarangayBayanihanOnline.Models


{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Submit()
        {
            if (ModelState.IsValid)
            {
                // Logic to save the data or send an email
                ViewBag.Message = "Message sent successfully!";
                ModelState.Clear();
            }
            else
            {
                ViewBag.Message = "Please fix the errors and try again.";
            }

            return View("Index");
        }
    }
}
