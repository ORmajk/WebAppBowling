using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAppBowling.Data;
using WebAppBowling.Models;

namespace WebAppBowling.Controllers
{
    // магазин и бар: каталог товаров, корзина, оформление заказа
    public class CatalogController : Controller
    {
        private readonly BowlingContext _context;

        public CatalogController(BowlingContext context)
        {
            _context = context;
        }

        // GET: /Catalog
        public IActionResult Index(string? category, string? search, string? sort)
        {
            var products = _context.Products.Where(p => p.IsVisible);

            if (!string.IsNullOrEmpty(category))
                products = products.Where(p => p.Category == category);

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.Name.Contains(search));

            switch (sort)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Category).ThenBy(p => p.Name);
                    break;
            }

            ViewBag.Categories = _context.Products.Where(p => p.IsVisible).Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
            ViewBag.Category = category;
            ViewBag.Search = search;
            ViewBag.Sort = sort;

            return View(products.ToList());
        }

        // POST: /Catalog/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int id, int quantity, string? category)
        {
            var product = _context.Products.Find(id);
            if (product == null || !product.IsVisible)
                return NotFound();

            if (quantity < 1) quantity = 1;

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            int inCart = item != null ? item.Quantity : 0;

            if (inCart + quantity > product.Quantity)
            {
                TempData["Error"] = "Товара «" + product.Name + "» осталось только " + product.Quantity + " шт.";
                return RedirectToAction("Index", new { category });
            }

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                });
            }
            else
            {
                item.Quantity += quantity;
            }

            SaveCart(cart);
            TempData["Message"] = "«" + product.Name + "» добавлен в корзину";
            return RedirectToAction("Index", new { category });
        }

        // GET: /Catalog/Cart
        public IActionResult Cart()
        {
            var cart = GetCart();
            ViewBag.Total = cart.Sum(c => c.Total);
            return View(cart);
        }

        // POST: /Catalog/UpdateCart - изменить количество
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(int id, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductId == id);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    var product = _context.Products.Find(id);
                    if (product != null && quantity > product.Quantity)
                    {
                        quantity = product.Quantity;
                        TempData["Error"] = "Товара «" + product.Name + "» осталось только " + product.Quantity + " шт.";
                    }
                    item.Quantity = quantity;
                    if (item.Quantity <= 0) cart.Remove(item);
                }
                SaveCart(cart);
            }
            return RedirectToAction("Cart");
        }

        // POST: /Catalog/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ProductId == id);
            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        // POST: /Catalog/Checkout - оформление заказа
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(string paymentMethod, string? comment)
        {
            int? clientId = HttpContext.Session.GetInt32("ClientId");
            if (clientId == null)
            {
                TempData["Message"] = "Чтобы оформить заказ, войдите или зарегистрируйтесь. Корзина сохранится.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Catalog/Cart" });
            }

            var cart = GetCart();
            if (cart.Count == 0)
            {
                TempData["Error"] = "Корзина пуста";
                return RedirectToAction("Cart");
            }

            if (paymentMethod != "Наличные" && paymentMethod != "Карта" && paymentMethod != "Онлайн")
                paymentMethod = "Наличные";

            // проверяем остатки и берём актуальные цены из базы
            var order = new Order
            {
                ClientId = clientId.Value,
                CreatedAt = DateTime.Now,
                PaymentMethod = paymentMethod,
                Status = "Новый",
                Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim()
            };

            foreach (var item in cart)
            {
                var product = _context.Products.Find(item.ProductId);
                if (product == null || !product.IsVisible)
                {
                    TempData["Error"] = "Товар «" + item.Name + "» больше не продаётся, удалите его из корзины";
                    return RedirectToAction("Cart");
                }
                if (product.Quantity < item.Quantity)
                {
                    TempData["Error"] = "Товара «" + product.Name + "» осталось только " + product.Quantity + " шт.";
                    return RedirectToAction("Cart");
                }

                product.Quantity -= item.Quantity;
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    Price = product.Price
                });
                order.TotalAmount += product.Price * item.Quantity;
            }

            // номер заказа ZK-0001, ZK-0002 ...
            int count = _context.Orders.Count() + 1;
            string number = "ZK-" + count.ToString("0000");
            while (_context.Orders.Any(o => o.OrderNumber == number))
            {
                count++;
                number = "ZK-" + count.ToString("0000");
            }
            order.OrderNumber = number;

            _context.Orders.Add(order);
            _context.SaveChanges();

            SaveCart(new List<CartItem>());
            TempData["Message"] = "Заказ " + number + " оформлен на сумму " + order.TotalAmount.ToString("N2") +
                " руб. Заберите его на стойке бара.";
            return RedirectToAction("Profile", "Account");
        }

        // корзина хранится в сессии в виде JSON
        private List<CartItem> GetCart()
        {
            string? json = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(json))
                return new List<CartItem>();
            return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));
            HttpContext.Session.SetInt32("CartCount", cart.Sum(c => c.Quantity));
        }
    }
}
