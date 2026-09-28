using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using WebAppBowling.Data;
using WebAppBowling.Models;

namespace WebAppBowling.Controllers
{
    // панель менеджера: брони, заказы, товары, дорожки, клиенты
    public class ManagerController : Controller
    {
        private readonly BowlingContext _context;
        private readonly IWebHostEnvironment _env;

        private readonly List<string> bookingStatuses = new List<string> { "Создан", "Оплачен", "Завершён", "Отменён" };
        private readonly List<string> orderStatuses = new List<string> { "Новый", "Готовится", "Выдан", "Отменён" };
        private readonly List<string> categories = new List<string> { "Напитки", "Еда", "Аксессуары", "Сертификаты" };

        public ManagerController(BowlingContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // сохраняет загруженное фото в wwwroot/images/products и возвращает путь к нему
        // если файл не подходит - возвращает null и пишет ошибку в ViewBag.PhotoError
        private string? SavePhoto(IFormFile? photo)
        {
            if (photo == null || photo.Length == 0)
                return null;

            string ext = Path.GetExtension(photo.FileName).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
            {
                ViewBag.PhotoError = "Можно загрузить только JPG, PNG или WEBP";
                return null;
            }
            if (photo.Length > 5 * 1024 * 1024)
            {
                ViewBag.PhotoError = "Файл слишком большой, максимум 5 МБ";
                return null;
            }

            string folder = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(folder);

            // имя файла делаем уникальным, чтобы не перезаписать чужое фото
            string fileName = "product_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 6) + ext;
            using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
            {
                photo.CopyTo(stream);
            }
            return "/images/products/" + fileName;
        }

        // перед каждым действием проверяем, что вошёл менеджер или администратор
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string? role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin")
            {
                context.Result = RedirectToAction("Login", "Account", new { returnUrl = "/Manager" });
                return;
            }
            base.OnActionExecuting(context);
        }

        // запись в журнал действий (таблица UserActions)
        private void AddLog(string type, string details)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return;
            _context.UserActions.Add(new UserAction
            {
                UserId = userId.Value,
                ActionType = type,
                Details = details,
                Timestamp = DateTime.Now
            });
            _context.SaveChanges();
        }

        // GET: /Manager - сводка
        public IActionResult Index()
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            ViewBag.BookingsToday = _context.Bookings.Count(b => b.StartTime >= today && b.StartTime < tomorrow && b.Status != "Отменён");
            ViewBag.NewBookings = _context.Bookings.Count(b => b.Status == "Создан" && b.StartTime >= today);
            ViewBag.NewOrders = _context.Orders.Count(o => o.Status == "Новый" || o.Status == "Готовится");
            ViewBag.BookingMoney = _context.Bookings.Where(b => b.Status != "Отменён").Sum(b => (decimal?)b.TotalAmount) ?? 0;
            ViewBag.OrderMoney = _context.Orders.Where(o => o.Status != "Отменён").Sum(o => (decimal?)o.TotalAmount) ?? 0;
            ViewBag.ClientsCount = _context.Clients.Count();
            ViewBag.LowStock = _context.Products.Where(p => p.IsVisible && p.Quantity < 5).OrderBy(p => p.Quantity).ToList();

            ViewBag.NearBookings = _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Lane)
                .Where(b => b.StartTime >= today && b.Status != "Отменён")
                .OrderBy(b => b.StartTime)
                .Take(8)
                .ToList();

            return View();
        }

        // ================= БРОНИРОВАНИЯ =================

        // GET: /Manager/Bookings
        public IActionResult Bookings(string? status, DateTime? date, string? search)
        {
            var bookings = _context.Bookings
                .Include(b => b.Client)
                .Include(b => b.Lane)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                bookings = bookings.Where(b => b.Status == status);

            if (date != null)
            {
                DateTime day = date.Value.Date;
                DateTime nextDay = day.AddDays(1);
                bookings = bookings.Where(b => b.StartTime >= day && b.StartTime < nextDay);
            }

            if (!string.IsNullOrEmpty(search))
                bookings = bookings.Where(b => b.BookingNumber.Contains(search) || b.Client!.FullName.Contains(search));

            ViewBag.Statuses = bookingStatuses;
            ViewBag.Status = status;
            ViewBag.Date = date;
            ViewBag.Search = search;

            return View(bookings.OrderByDescending(b => b.StartTime).ToList());
        }

        // POST: /Manager/BookingStatus - смена статуса брони
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookingStatus(int id, string status)
        {
            var booking = _context.Bookings.Find(id);
            if (booking == null)
                return NotFound();

            if (!bookingStatuses.Contains(status))
            {
                TempData["Error"] = "Неизвестный статус";
                return RedirectToAction("Bookings");
            }

            string oldStatus = booking.Status;
            booking.Status = status;
            _context.SaveChanges();
            AddLog("Редактирование", "Бронь " + booking.BookingNumber + ": статус «" + oldStatus + "» → «" + status + "» (сайт)");

            TempData["Message"] = "Статус брони " + booking.BookingNumber + " изменён на «" + status + "»";
            return RedirectToAction("Bookings");
        }

        // ================= ЗАКАЗЫ =================

        // GET: /Manager/Orders
        public IActionResult Orders(string? status)
        {
            var orders = _context.Orders
                .Include(o => o.Client)
                .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                orders = orders.Where(o => o.Status == status);

            ViewBag.Statuses = orderStatuses;
            ViewBag.Status = status;

            return View(orders.OrderByDescending(o => o.CreatedAt).ToList());
        }

        // POST: /Manager/OrderStatus - смена статуса заказа
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult OrderStatus(int id, string status)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems).ThenInclude(i => i.Product)
                .FirstOrDefault(o => o.Id == id);
            if (order == null)
                return NotFound();

            if (!orderStatuses.Contains(status))
            {
                TempData["Error"] = "Неизвестный статус";
                return RedirectToAction("Orders");
            }

            // если заказ отменили - возвращаем товары на склад
            if (status == "Отменён" && order.Status != "Отменён")
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null) item.Product.Quantity += item.Quantity;
                }
            }
            // если отменённый заказ вернули в работу - снова списываем
            else if (order.Status == "Отменён" && status != "Отменён")
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null && item.Product.Quantity < item.Quantity)
                    {
                        TempData["Error"] = "Не хватает товара «" + item.Product.Name + "» на складе";
                        return RedirectToAction("Orders");
                    }
                }
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null) item.Product.Quantity -= item.Quantity;
                }
            }

            string oldStatus = order.Status;
            order.Status = status;
            _context.SaveChanges();
            AddLog("Редактирование", "Заказ " + order.OrderNumber + ": статус «" + oldStatus + "» → «" + status + "» (сайт)");

            TempData["Message"] = "Статус заказа " + order.OrderNumber + " изменён на «" + status + "»";
            return RedirectToAction("Orders");
        }

        // ================= ТОВАРЫ =================

        // GET: /Manager/Products
        public IActionResult Products()
        {
            return View(_context.Products.OrderBy(p => p.Category).ThenBy(p => p.Name).ToList());
        }

        // GET: /Manager/ProductCreate
        public IActionResult ProductCreate()
        {
            ViewBag.Categories = categories;
            return View("ProductForm", new Product());
        }

        // POST: /Manager/ProductCreate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProductCreate(Product product, IFormFile? photo)
        {
            string? photoPath = SavePhoto(photo);
            if (ViewBag.PhotoError != null)
                ModelState.AddModelError("", (string)ViewBag.PhotoError);

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = categories;
                return View("ProductForm", product);
            }

            if (photoPath != null)
                product.ImageUrl = photoPath;

            _context.Products.Add(product);
            _context.SaveChanges();
            AddLog("Добавление", "Добавлен товар: " + product.Name + " (сайт)");

            TempData["Message"] = "Товар «" + product.Name + "» добавлен";
            return RedirectToAction("Products");
        }

        // GET: /Manager/ProductEdit/5
        public IActionResult ProductEdit(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = categories;
            return View("ProductForm", product);
        }

        // POST: /Manager/ProductEdit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProductEdit(Product product, IFormFile? photo)
        {
            string? photoPath = SavePhoto(photo);
            if (ViewBag.PhotoError != null)
                ModelState.AddModelError("", (string)ViewBag.PhotoError);

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = categories;
                return View("ProductForm", product);
            }

            if (photoPath != null)
                product.ImageUrl = photoPath;

            _context.Products.Update(product);
            _context.SaveChanges();
            AddLog("Редактирование", "Изменён товар: " + product.Name + " (сайт)");

            TempData["Message"] = "Товар «" + product.Name + "» сохранён";
            return RedirectToAction("Products");
        }

        // POST: /Manager/ProductDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProductDelete(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            // если товар уже есть в заказах - удалять нельзя, просто скрываем с сайта
            if (_context.OrderItems.Any(i => i.ProductId == id))
            {
                product.IsVisible = false;
                _context.SaveChanges();
                TempData["Message"] = "Товар «" + product.Name + "» есть в заказах, поэтому он не удалён, а скрыт с сайта";
            }
            else
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
                TempData["Message"] = "Товар «" + product.Name + "» удалён";
            }

            AddLog("Удаление", "Удалён/скрыт товар: " + product.Name + " (сайт)");
            return RedirectToAction("Products");
        }

        // ================= ДОРОЖКИ =================

        // GET: /Manager/Lanes
        public IActionResult Lanes()
        {
            ViewBag.Statuses = _context.LaneStatuses.OrderBy(s => s.Id).ToList();
            return View(_context.Lanes
                .Include(l => l.LaneType)
                .Include(l => l.Status)
                .OrderBy(l => l.LaneNumber)
                .ToList());
        }

        // POST: /Manager/LaneUpdate - изменить состояние и цену дорожки
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LaneUpdate(int id, int statusId, string price)
        {
            var lane = _context.Lanes.Find(id);
            if (lane == null)
                return NotFound();

            decimal newPrice;
            if (!decimal.TryParse(price.Replace('.', ','), out newPrice) || newPrice <= 0)
            {
                TempData["Error"] = "Цена указана неправильно";
                return RedirectToAction("Lanes");
            }

            if (!_context.LaneStatuses.Any(s => s.Id == statusId))
            {
                TempData["Error"] = "Неизвестное состояние дорожки";
                return RedirectToAction("Lanes");
            }

            lane.StatusId = statusId;
            lane.PricePerHour = newPrice;
            _context.SaveChanges();
            AddLog("Редактирование", "Отредактирована дорожка №" + lane.LaneNumber + " (сайт)");

            TempData["Message"] = "Дорожка №" + lane.LaneNumber + " сохранена";
            return RedirectToAction("Lanes");
        }

        // ================= КЛИЕНТЫ =================

        // GET: /Manager/Clients
        public IActionResult Clients(string? search)
        {
            var clients = _context.Clients
                .Include(c => c.Bookings)
                .Include(c => c.Orders)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                clients = clients.Where(c => c.FullName.Contains(search) || (c.Phone != null && c.Phone.Contains(search)));

            ViewBag.Search = search;
            return View(clients.OrderBy(c => c.FullName).ToList());
        }
    }
}
