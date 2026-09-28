using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBowling.Data;
using WebAppBowling.Models;

namespace WebAppBowling.Controllers
{
    public class HomeController : Controller
    {
        private readonly BowlingContext _context;

        public HomeController(BowlingContext context)
        {
            _context = context;
        }

        // главная страница: категории дорожек с ценами и ближайшие мероприятия
        public IActionResult Index()
        {
            ViewBag.LaneTypes = _context.LaneTypes
                .Include(t => t.Lanes)
                .ToList();

            ViewBag.Events = _context.Events
                .Where(e => e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .Take(3)
                .ToList();

            ViewBag.Products = _context.Products
                .Where(p => p.IsVisible && p.Quantity > 0)
                .OrderBy(p => p.Price)
                .Take(4)
                .ToList();

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
