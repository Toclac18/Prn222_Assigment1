using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NewsManagementSystem.Models;
using NewsManagementSystem.Service;

namespace NewsManagementSystem.Controllers
{
    public class NewsArticleController : Controller
    {
        private readonly FunewsManagementContext _context;
        private readonly IEmailService _emailService;
        public NewsArticleController(FunewsManagementContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }
        public ActionResult NewsArticleList()
        {

            var listNewsArticle = _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .ToList();

            return View(listNewsArticle);
        }


        public ActionResult NewsArticleDetail(int id)
        {
            var newsArticle = _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .FirstOrDefault(n => n.NewsArticleId == id.ToString()); 

            if (newsArticle == null)
            {
                return NotFound(); 
            }

            return View(newsArticle);
        }

        public ActionResult NewsArticleCreate()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewsArticleCreate(NewsArticle model)
        {
            model.CreatedDate = DateTime.Now;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Errors"] = string.Join("; ", errors);
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName");
                return View(model);
            }

            try
            {
                // ✅ Kiểm tra ID đã tồn tại hay chưa
                if (_context.NewsArticles.Any(n => n.NewsArticleId == model.NewsArticleId))
                {
                    ModelState.AddModelError("NewsArticleId", "ID này đã tồn tại. Vui lòng nhập ID khác.");
                    ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName");
                    return View(model);
                }

                var userId = HttpContext.Session.GetInt32("UserID");
                if (userId == null)
                {
                    ModelState.AddModelError("", "User chưa đăng nhập. Vui lòng đăng nhập lại.");
                    ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName");
                    return View(model);
                }

                model.CreatedById = (short)userId.Value;
                model.UpdatedById = null;
                model.NewsStatus = true;
                model.ModifiedDate = null;

                _context.NewsArticles.Add(model);
                await _context.SaveChangesAsync(); // ✅ Dùng await

                // ✅ Lấy tên tác giả an toàn
                string authorName = _context.SystemAccounts
                    .Where(u => u.AccountId == model.CreatedById)
                    .Select(u => u.AccountName)
                    .FirstOrDefault() ?? "Unknown";

                // ✅ Gửi email thông báo cho Admin
                string adminEmail = "admin@gmail.com";
                string subject = "🆕 New Article Published!";
                string body = $@"
            <h3>New Article: {model.NewsTitle}</h3>
            <p><b>Author:</b> {authorName}</p>
            <p>Click <a href='http://localhost:5130/Home/DetailNewsArticle/{model.NewsArticleId}'>here</a> to view the article.</p>";

                try
                {
                    await _emailService.SendEmailAsync(adminEmail, subject, body);
                }
                catch (Exception emailEx)
                {
                    TempData["EmailError"] = "Bài viết đã tạo, nhưng email không gửi được: " + emailEx.Message;
                }

                TempData["UpdateDone"] = "News Article đã được tạo thành công!";
                return RedirectToAction(nameof(NewsArticleList));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi tạo bài viết: " + ex.Message);
                ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName");
                return View(model);
            }
        }


        public ActionResult NewsArticleUpdate(int id)
        {
            var newsArticle = _context.NewsArticles
                .Include(n => n.Category)
                .Include(n => n.CreatedBy)
                .FirstOrDefault(n => n.NewsArticleId == id.ToString());

            if (newsArticle == null)
            {
                return NotFound();
            }

            ViewBag.CategoryId = _context.Categories.Any()
                ? new SelectList(_context.Categories, "CategoryId", "CategoryName", newsArticle.CategoryId)
                : new SelectList(new List<Category>());

            ViewBag.CreatedById = _context.SystemAccounts.Any()
                ? new SelectList(_context.SystemAccounts, "AccountId", "AccountName", newsArticle.CreatedById)
                : new SelectList(new List<SystemAccount>());

            return View(newsArticle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewsArticleUpdate(string id, NewsArticle model) // Đổi kiểu int -> string
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Nếu có lỗi, quay lại form với thông báo lỗi
            }
            try
            {
                var existingArticle = _context.NewsArticles.Find(id); // id giờ là string
                if (existingArticle == null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin bài viết
                existingArticle.NewsTitle = model.NewsTitle;
                existingArticle.Headline = model.Headline;
                existingArticle.NewsContent = model.NewsContent;
                existingArticle.NewsSource = model.NewsSource;
                existingArticle.CategoryId = model.CategoryId;
                existingArticle.NewsStatus = model.NewsStatus;
                existingArticle.ModifiedDate = DateTime.Now;

                // Kiểm tra session Role
                var userRole = HttpContext.Session.GetInt32("Role");
                var userId = HttpContext.Session.GetInt32("UserID");

                if (userRole != null && userRole != 0) // Nếu không phải admin
                {
                    if (userId != null)
                    {
                        existingArticle.UpdatedById = (short?)userId.Value;
                    }
                }

                // Lưu vào DB
                _context.NewsArticles.Update(existingArticle);
                _context.SaveChanges();
                TempData["UpdateDone"] = "Đã cập nhập thành công";
                return RedirectToAction("NewsArticleList");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi cập nhật: {ex.Message}");
                ModelState.AddModelError("", ex.Message + "Có lỗi xảy ra khi cập nhật. Vui lòng thử lại!");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(string id)
        {
            try
            {
                var article = _context.NewsArticles.FirstOrDefault(a => a.NewsArticleId == id);
                if (article == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy bài viết!";
                    return RedirectToAction(nameof(NewsArticleList));
                }

                _context.NewsArticles.Remove(article);
                _context.SaveChanges();

                TempData["UpdateDone"] = "Xóa bài viết thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa bài viết: " + ex.Message;
            }

            return RedirectToAction(nameof(NewsArticleList));
        }
    }
}
