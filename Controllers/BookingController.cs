using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebAppBowling.Data;
using WebAppBowling.Models;

namespace WebAppBowling.Controllers
{
    public class BookingController : Controller
    {
        private readonly BowlingContext _context;

        // клуб работает с 10:00 до 24:00
        private const int OpenHour = 10;
        private const int CloseHour = 24;

        public BookingController(BowlingContext context)
        {
            _context = context;
        }

        // GET: /Booking?date=2026-09-30 - дорожки и занятое время на выбранный день
        public IActionResult Index(DateTime? date)
        {
            DateTime day = (date ?? DateTime.Today).Date;
            if (day < DateTime.Today) day = DateTime.Today;

            var lanes = _context.Lanes
                .Include(l => l.LaneType)
                .Include(l => l.Status)
                .OrderBy(l => l.LaneNumber)
                .ToList();

            DateTime nextDay = day.AddDays(1);
            var bookings = _context.Bookings
                .Where(b => b.StartTime >= day && b.StartTime < nextDay && b.Status != "Отменён")
                .OrderBy(b => b.StartTime)
                .ToList();

            ViewBag.Date = day;
            ViewBag.Bookings = bookings;
            return View(lanes);
        }

        // GET: /Booking/Create
        public IActionResult Create(int? laneId, DateTime? date)
        {
            if (HttpContext.Session.GetInt32("ClientId") == null)
            {
                TempData["Message"] = "Чтобы забронировать дорожку, войдите или зарегистрируйтесь.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Booking/Create" });
            }

            var form = new BookingForm
            {
                LaneId = laneId,
                Date = date ?? DateTime.Today
            };

            FillLists();
            return View(form);
        }

        // POST: /Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookingForm form)
        {
            int? clientId = HttpContext.Session.GetInt32("ClientId");
            if (clientId == null)
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                FillLists();
                return View(form);
            }

            // собираем дату и время начала
            DateTime time;
            if (!DateTime.TryParseExact(form.Time, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out time))
            {
                ModelState.AddModelError("Time", "Время указано неправильно");
                FillLists();
                return View(form);
            }

            DateTime start = form.Date!.Value.Date.Add(time.TimeOfDay);
            DateTime end = start.AddMinutes(form.DurationMinutes);

            if (start < DateTime.Now)
                ModelState.AddModelError("", "Нельзя забронировать дорожку на прошедшее время");

            if (start > DateTime.Today.AddMonths(2))
                ModelState.AddModelError("", "Бронировать можно не больше чем на 2 месяца вперёд");

            if (start.Hour < OpenHour || end > start.Date.AddHours(CloseHour))
                ModelState.AddModelError("", "Клуб работает с 10:00 до 24:00, игра должна закончиться до полуночи");

            var lane = _context.Lanes
                .Include(l => l.Status)
                .Include(l => l.LaneType)
                .FirstOrDefault(l => l.Id == form.LaneId);

            if (lane == null)
                ModelState.AddModelError("LaneId", "Дорожка не найдена");
            else if (lane.Status != null && lane.Status.Name == "Требует обслуживания")
                ModelState.AddModelError("LaneId", "Эта дорожка на обслуживании, выберите другую");

            // проверяем, не занята ли дорожка в это время
            if (lane != null)
            {
                var laneBookings = _context.Bookings
                    .Where(b => b.LaneId == lane.Id && b.Status != "Отменён")
                    .ToList();

                foreach (var b in laneBookings)
                {
                    if (start < b.EndTime && end > b.StartTime)
                    {
                        ModelState.AddModelError("", "Дорожка №" + lane.LaneNumber + " уже занята с " +
                            b.StartTime.ToString("HH:mm") + " до " + b.EndTime.ToString("HH:mm") + ". Выберите другое время.");
                        break;
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                FillLists();
                return View(form);
            }

            // номер брони BR-001, BR-002 ...
            int count = _context.Bookings.Count() + 1;
            string number = "BR-" + count.ToString("000");
            while (_context.Bookings.Any(b => b.BookingNumber == number))
            {
                count++;
                number = "BR-" + count.ToString("000");
            }

            var booking = new Booking
            {
                BookingNumber = number,
                ClientId = clientId.Value,
                LaneId = lane!.Id,
                StartTime = start,
                DurationMinutes = form.DurationMinutes,
                TotalAmount = Math.Round(lane.PricePerHour * form.DurationMinutes / 60, 2),
                PaymentMethod = form.PaymentMethod,
                Status = "Создан"
            };
            _context.Bookings.Add(booking);

            // 10 баллов клиенту за бронь
            var client = _context.Clients.Find(clientId.Value);
            if (client != null) client.Points += 10;

            _context.SaveChanges();

            TempData["Message"] = "Дорожка №" + lane.LaneNumber + " забронирована на " + start.ToString("dd.MM.yyyy HH:mm") +
                ". Номер брони: " + number + ", сумма: " + booking.TotalAmount.ToString("N2") + " руб.";
            return RedirectToAction("Profile", "Account");
        }

        // POST: /Booking/Cancel/5 - клиент отменяет свою бронь
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancel(int id)
        {
            int? clientId = HttpContext.Session.GetInt32("ClientId");
            if (clientId == null)
                return RedirectToAction("Login", "Account");

            var booking = _context.Bookings.FirstOrDefault(b => b.Id == id && b.ClientId == clientId);
            if (booking == null)
                return NotFound();

            if (booking.Status != "Создан" && booking.Status != "Оплачен")
            {
                TempData["Error"] = "Эту бронь уже нельзя отменить";
            }
            else if (booking.StartTime <= DateTime.Now)
            {
                TempData["Error"] = "Игра уже началась, отменить бронь нельзя";
            }
            else
            {
                booking.Status = "Отменён";
                _context.SaveChanges();
                TempData["Message"] = "Бронь " + booking.BookingNumber + " отменена";
            }

            return RedirectToAction("Profile", "Account");
        }

        // списки для выпадающих полей формы
        private void FillLists()
        {
            var lanes = _context.Lanes
                .Include(l => l.LaneType)
                .Include(l => l.Status)
                .Where(l => l.Status!.Name != "Требует обслуживания")
                .OrderBy(l => l.LaneNumber)
                .ToList();

            ViewBag.Lanes = lanes.Select(l => new SelectListItem
            {
                Value = l.Id.ToString(),
                Text = "Дорожка №" + l.LaneNumber + " — " + l.LaneType!.Name + ", до " + l.Capacity + " чел., " +
                       l.PricePerHour.ToString("N0") + " руб./час"
            }).ToList();

            var times = new List<string>();
            for (int h = OpenHour; h < CloseHour; h++)
            {
                times.Add(h.ToString("00") + ":00");
                times.Add(h.ToString("00") + ":30");
            }
            ViewBag.Times = new SelectList(times);

            ViewBag.Durations = new List<SelectListItem>
            {
                new SelectListItem { Value = "60", Text = "1 час" },
                new SelectListItem { Value = "90", Text = "1,5 часа" },
                new SelectListItem { Value = "120", Text = "2 часа" },
                new SelectListItem { Value = "180", Text = "3 часа" }
            };

            ViewBag.Payments = new SelectList(new List<string> { "Наличные", "Карта", "Онлайн" });
        }
    }
}
