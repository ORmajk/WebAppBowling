using Microsoft.AspNetCore.Mvc;
using WebAppBowling.Models;

namespace WebAppBowling.Controllers
{
    public class CatalogController : Controller
    {
        private readonly BowlingClubContext _context;

        public CatalogController(BowlingClubContext context)
        {
            _context = context;
        }

        // GET: /Catalog
        public async Task<IActionResult> Index(string? search, int? categoryId, int? tagId,
                                                string? sort, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Manufacturer)
                .Include(p => p.TagMappings)
                    .ThenInclude(tm => tm.Tag)
                .Where(p => p.IsVisible && p.AvailabilityStatus != 2); // Не показываем товары "Нет в наличии"

            // Поиск по названию или артикулу
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Article.Contains(search));
                ViewBag.SearchTerm = search;
            }

            // Фильтрация по цене
            if (minPrice.HasValue)
                query = query.Where(p => p.ActualPrice >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.ActualPrice <= maxPrice.Value);

            // Фильтрация по тегу
            if (tagId.HasValue)
            {
                query = query.Where(p => p.TagMappings.Any(tm => tm.TagId == tagId.Value));
                ViewBag.SelectedTag = await _context.ProductTags.FindAsync(tagId.Value);
            }

            // Сортировка
            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.ActualPrice),
                "price_desc" => query.OrderByDescending(p => p.ActualPrice),
                "name_asc" => query.OrderBy(p => p.Name),
                "name_desc" => query.OrderByDescending(p => p.Name),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name)
            };

            var products = await query.ToListAsync();
            var tags = await _context.ProductTags.ToListAsync();

            var viewModel = new CatalogViewModel
            {
                Products = products,
                Tags = tags,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sort
            };

            return View(viewModel);
        }

        // GET: /Catalog/Product/{id}
        public async Task<IActionResult> Product(int id)
        {
            var product = await _context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Manufacturer)
                .Include(p => p.TagMappings)
                    .ThenInclude(tm => tm.Tag)
                .Include(p => p.Comments.Where(c => c.ModerationStatus == 1))
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null || !product.IsVisible)
                return NotFound();

            // Получаем похожие товары (с теми же тегами)
            var tagIds = product.TagMappings.Select(tm => tm.TagId).ToList();
            var relatedProducts = await _context.Products
                .Include(p => p.TagMappings)
                .Where(p => p.Id != id && p.IsVisible &&
                           p.TagMappings.Any(tm => tagIds.Contains(tm.TagId)))
                .Take(4)
                .ToListAsync();

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                NewComment = new ProductComment { ProductId = id }
            };

            return View(viewModel);
        }

        // POST: /Catalog/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(ProductComment comment)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Catalog/Product/{comment.ProductId}" });

            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirst("UserId").Value);
                comment.UserId = userId;
                comment.CreatedAt = DateTime.Now;
                comment.ModerationStatus = 0; // На модерации

                _context.ProductComments.Add(comment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Ваш отзыв отправлен на модерацию и скоро появится на сайте.";
            }

            return RedirectToAction("Product", new { id = comment.ProductId });
        }
    }
}
