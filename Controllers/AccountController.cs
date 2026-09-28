using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBowling.Data;
using WebAppBowling.Models;

namespace WebAppBowling.Controllers
{
    public class AccountController : Controller
    {
        private readonly BowlingContext _context;

        public AccountController(BowlingContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login(string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Account/Login
        // один вход и для клиентов, и для менеджеров
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string login, string password, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Введите логин и пароль";
                return View();
            }

            // сначала ищем среди сотрудников
            var user = _context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
            if (user != null)
            {
                if (user.Role != "Manager" && user.Role != "Admin")
                {
                    ViewBag.Error = "На сайт могут входить только менеджеры и администраторы";
                    return View();
                }

                HttpContext.Session.Clear();
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("Role", user.Role);

                _context.UserActions.Add(new UserAction
                {
                    UserId = user.Id,
                    ActionType = "Вход",
                    Details = "Вход на сайт: " + user.Login,
                    Timestamp = DateTime.Now
                });
                _context.SaveChanges();

                return RedirectToAction("Index", "Manager");
            }

            // потом среди клиентов
            var client = _context.Clients.FirstOrDefault(c => c.Login == login && c.Password == password);
            if (client != null)
            {
                HttpContext.Session.Remove("UserId");
                HttpContext.Session.Remove("Role");
                HttpContext.Session.SetInt32("ClientId", client.Id);
                HttpContext.Session.SetString("ClientName", client.FullName);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Profile");
            }

            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            return View(new RegisterForm());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterForm form)
        {
            if (_context.Clients.Any(c => c.Login == form.Login) || _context.Users.Any(u => u.Login == form.Login))
                ModelState.AddModelError("Login", "Такой логин уже занят");

            if (!ModelState.IsValid)
                return View(form);

            var client = new Client
            {
                FullName = form.FullName.Trim(),
                Phone = form.Phone.Trim(),
                Email = string.IsNullOrWhiteSpace(form.Email) ? null : form.Email.Trim(),
                Login = form.Login.Trim(),
                Password = form.Password,
                RegistrationDate = DateTime.Today,
                Points = 0,
                IsSubscribedToAl = false
            };

            _context.Clients.Add(client);
            _context.SaveChanges();

            HttpContext.Session.SetInt32("ClientId", client.Id);
            HttpContext.Session.SetString("ClientName", client.FullName);

            TempData["Message"] = "Регистрация прошла успешно! Теперь можно бронировать дорожки и заказывать товары.";
            return RedirectToAction("Profile");
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile - личный кабинет клиента
        public IActionResult Profile()
        {
            int? clientId = HttpContext.Session.GetInt32("ClientId");
            if (clientId == null)
                return RedirectToAction("Login", new { returnUrl = "/Account/Profile" });

            var client = _context.Clients.Find(clientId);
            if (client == null)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login");
            }

            ViewBag.Bookings = _context.Bookings
                .Include(b => b.Lane).ThenInclude(l => l!.LaneType)
                .Where(b => b.ClientId == clientId)
                .OrderByDescending(b => b.StartTime)
                .ToList();

            ViewBag.Orders = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .Where(o => o.ClientId == clientId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(client);
        }
    }
}
