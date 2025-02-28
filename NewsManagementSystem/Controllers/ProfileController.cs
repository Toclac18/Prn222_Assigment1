using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsManagementSystem.Models;

namespace NewsManagementSystem.Controllers
{
    public class ProfileController : Controller
    {
        private readonly FunewsManagementContext _context;

        public ProfileController(FunewsManagementContext context)
        {
            _context = context;
        }
        public ActionResult ViewProfile()
        {

            // Lấy UserID từ Session
            short? userId = (short?)HttpContext.Session.GetInt32("UserID");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Account"); // Chuyển hướng về trang đăng nhập nếu chưa đăng nhập
            }

            // Tìm user theo ID
            var user = _context.SystemAccounts.FirstOrDefault(u => u.AccountId == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return View(user);
        }


        public ActionResult UpdateProfile()
        {
            // Lấy UserID từ Session
            short? userId = (short?)HttpContext.Session.GetInt32("UserID");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Account"); // Chuyển hướng về trang đăng nhập nếu chưa đăng nhập
            }

            // Lấy thông tin user từ database
            var user = _context.SystemAccounts.FirstOrDefault(u => u.AccountId == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            return View(user); // Trả về form UpdateProfile với thông tin user
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(SystemAccount model)
        {
            // Lấy UserID từ Session
            short? userId = (short?)HttpContext.Session.GetInt32("UserID");

            if (userId == null || userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // Tìm user theo ID trong database
            var user = _context.SystemAccounts.FirstOrDefault(u => u.AccountId == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(model.AccountName))
            {
                ModelState.AddModelError("AccountName", "Account Name cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(model.AccountPassword))
            {
                ModelState.AddModelError("AccountPassword", "Password cannot be empty.");
            }

            // Nếu có lỗi, hiển thị lại form với thông báo lỗi
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Chỉ cập nhật nếu có dữ liệu hợp lệ
                user.AccountName = model.AccountName;
                user.AccountPassword = model.AccountPassword;

                _context.SaveChanges(); // Lưu thay đổi vào database    
                HttpContext.Session.SetString("Username", user.AccountName);

                TempData["SuccessMessage"] = "Profile updated successfully!";
                return RedirectToAction("UpdateProfile"); // Quay về trang hồ sơ sau khi cập nhật
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating profile: " + ex.Message);
                return View(model);
            }
        }
    }
}
