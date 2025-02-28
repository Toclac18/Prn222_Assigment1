using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NewsManagementSystem.Models;

namespace NewsManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FunewsManagementContext _context;

        public HomeController(ILogger<HomeController> logger, FunewsManagementContext context)
        {
            _logger = logger;
            _context = context;
        }

        public ActionResult NewsArticle(short? categoryId, string? sortOrder, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                ModelState.AddModelError("", "End Date must be greater than Start Date!");
            }

            var newsArticles = _context.NewsArticles
                .Where(n => n.NewsStatus == true) // Chỉ lấy bài viết có NewsStatus == true
                .AsQueryable();

            // Lọc theo CategoryId (nếu có)
            if (categoryId.HasValue)
            {
                newsArticles = newsArticles.Where(n => n.CategoryId == categoryId);
            }

            // Lọc theo khoảng thời gian (CreatedDate)
            if (startDate.HasValue)
            {
                newsArticles = newsArticles.Where(n => n.CreatedDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                newsArticles = newsArticles.Where(n => n.CreatedDate <= endDate.Value);
            }

            // Sắp xếp theo ngày tạo
            switch (sortOrder)
            {
                case "date_asc":
                    newsArticles = newsArticles.OrderBy(n => n.CreatedDate);
                    break;
                default: // "date_desc" hoặc null
                    newsArticles = newsArticles.OrderByDescending(n => n.CreatedDate);
                    break;
            }

            // Chỉ lấy danh mục có IsActive == true
            ViewBag.Categories = _context.Categories
                .Where(c => c.IsActive == true)
                .Select(c => new { c.CategoryId, c.CategoryName })
                .ToList();

            // Truyền dữ liệu về View
            ViewBag.SortOrder = sortOrder;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(newsArticles.ToList());
        }


        public ActionResult DetailNewsArticle(string id)
        {
            var newsArticle = _context.NewsArticles.FirstOrDefault(n => n.NewsArticleId == id);
            if (newsArticle == null)
            {
                return NotFound();
            }

            // Chỉ lấy AccountName nếu UpdatedById có giá trị
            string? updatedByName = null;
            if (newsArticle.UpdatedById.HasValue) // Kiểm tra nếu UpdatedById có giá trị
            {
                updatedByName = _context.SystemAccounts
                    .Where(a => a.AccountId == newsArticle.UpdatedById.Value) // Ép kiểu từ short? -> short
                    .Select(a => a.AccountName)
                    .FirstOrDefault();
            }

            ViewBag.UpdatedByName = updatedByName;

            return View(newsArticle);
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
