using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SunTech.Models;

namespace SunTech.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Product()
        {
            // Redirect to Products controller Index action
            return RedirectToAction("Index", "Products");
        }


    }
}
