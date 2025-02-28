using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsManagementSystem.Models;

namespace NewsManagementSystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly FunewsManagementContext _context;
        private readonly IConfiguration _configuration;

        public LoginController(FunewsManagementContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("Username"); // Xóa Session
            return RedirectToAction("NewsArticle", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(SystemAccount model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Vui lòng nhập đủ thông tin!";
                return View(model);
            }

            // Kiểm tra tài khoản Admin từ appsettings.json
            var adminEmail = _configuration["AdminAccount:Email"];
            var adminPassword = _configuration["AdminAccount:Password"];

            if (model.AccountName == adminEmail && model.AccountPassword == adminPassword)
            {
                HttpContext.Session.SetString("Username", adminEmail);
                HttpContext.Session.SetString("Role", "0");
                return RedirectToAction("NewsArticle", "Home");
            }

            // Kiểm tra trong database
            var user = _context.SystemAccounts
                .FirstOrDefault(u => u.AccountName == model.AccountName && u.AccountPassword == model.AccountPassword);

            if (user == null)
            {
                ViewBag.Message = "Sai tài khoản hoặc mật khẩu!";
                return View(model);
            }

            if (!user.IsActive)
            {
                ViewBag.Message = "Tài khoản này đã bị vô hiệu hóa!";
                return View(model);
            }

            // Lưu thông tin đăng nhập vào Session
            HttpContext.Session.SetInt32("UserID", user.AccountId);
            HttpContext.Session.SetString("Username", user.AccountName);
            HttpContext.Session.SetString("Role", user.AccountRole?.ToString() ?? "1"); // Mặc định là Staff

            return RedirectToAction("NewsArticle", "Home");
        }
    }
}
