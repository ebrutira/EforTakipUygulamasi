using Microsoft.AspNetCore.Mvc;
using EforTakipUygulamasi.Models;
using System.Diagnostics;

namespace EforTakipUygulamasi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Dashboard'a yönlendir
            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
};