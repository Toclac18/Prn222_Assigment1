using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NewsManagementSystem.Models;

namespace NewsManagementSystem.Controllers
{
    public class MAccountController : Controller
    {
        private readonly FunewsManagementContext _context;

        public MAccountController(FunewsManagementContext context)
        {
            _context = context;
        }
        public ActionResult Accountlist()
        {
            var users = _context.SystemAccounts.ToList();
            return View(users);
        }

        public ActionResult ToggleAccountStatus(int id)
        {
            short shortId = (short)id; // Ép kiểu về short vì lưu trữ trong db là short 
            var account = _context.SystemAccounts.Find(shortId);
            if (account == null)
            {
                return NotFound();
            }

            account.IsActive = !account.IsActive;
            _context.SaveChanges();

            return RedirectToAction("Accountlist"); // Chuyển hướng về danh sách
        }

        public ActionResult Details(int id)
        {
            short shortId = (short)id; // Ép kiểu về short vì lưu trữ trong db là short 
            var account = _context.SystemAccounts.Find(shortId);
            return View(account);
        }

        public ActionResult Update(int id)
        {
            short shortId = (short)id;
            var users = _context.SystemAccounts.Find(shortId);
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Update(SystemAccount model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại!";
                return View(model);
            }

            var existingAccount = _context.SystemAccounts.Find(model.AccountId);
            if (existingAccount == null)
            {
                TempData["ErrorMessage"] = "Tài khoản không tồn tại!";
                return NotFound();
            }

            try
            {
                // Cập nhật thông tin tài khoản
                existingAccount.AccountName = model.AccountName;
                existingAccount.AccountEmail = model.AccountEmail;
                existingAccount.AccountRole = model.AccountRole;
                existingAccount.AccountPassword = model.AccountPassword;
                existingAccount.IsActive = model.IsActive;

                _context.SaveChanges(); // Lưu thay đổi vào database

                TempData["SuccessMessage"] = "Cập nhật tài khoản thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật tài khoản: " + ex.Message;
                return View(model);
            }

            return RedirectToAction("Accountlist"); // Quay về danh sách tài khoản
        }
    }
}
