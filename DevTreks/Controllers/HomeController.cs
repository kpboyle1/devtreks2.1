using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DevTreks.Models;
using DevTreks.Helpers;

namespace DevTreks.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
            //2.0.0 localization code moved to Startup.cs
        }
       
        public IActionResult Index()
        {
            ViewData["Title"] = AppHelper.GetResource("DEVTREKS_TITLE");
            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = AppHelper.GetResource("DEVTREKS_ABOUT");
            ViewData["Goal"] = AppHelper.GetResource("DEVTREKS_GOAL");
            ViewData["Message"] = AppHelper.GetResource("SOCIALBUDGET_DOES");
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Please first review the tutorials before contacting DevTreks.";

            return View();
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
}
