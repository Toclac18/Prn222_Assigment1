using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewsManagementSystem.Models;

namespace NewsManagementSystem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly FunewsManagementContext _context;

        public CategoryController(FunewsManagementContext context)
        {
            _context = context;
        }
        // GET: get list Category
        public ActionResult ListCategory()
        {
            var listCate = _context.Categories.ToList();
            return View(listCate);
        }
        public ActionResult DetailCategory(short id)
        {
            var category = _context.Categories.Find(id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }


        // GET: Hiển thị form tạo danh mục
        public ActionResult CreateCategory()
        {
            var categories = _context.Categories.ToList();

            // Thêm option "No Parent Category" vào danh sách
            var categoryList = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "No Parent Category" } // Giá trị 0 tượng trưng cho không có danh mục cha
            };

            // Thêm các danh mục có sẵn
            categoryList.AddRange(categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            }));

            ViewBag.ParentCategoryId = categoryList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                category.IsActive = true;

                // Nếu chọn "No Parent Category", đặt tạm là null
                bool isNoParent = category.ParentCategoryId == 0;
                if (isNoParent)
                {
                    category.ParentCategoryId = null;
                }

                _context.Categories.Add(category);
                _context.SaveChanges(); // Lưu để lấy CategoryId

                // Nếu lúc đầu chọn "No Parent Category", cập nhật ParentCategoryId = chính nó
                if (isNoParent)
                {
                    category.ParentCategoryId = category.CategoryId;
                    _context.Categories.Update(category);
                    _context.SaveChanges(); // Lưu lần nữa
                }
                TempData["CreateDone"] = "Đã thêm mới category thành công";
                return RedirectToAction(nameof(ListCategory));
            }

            return RedirectToAction("CreateCategory", "Category");
        }



        // GET: Hiển thị form cập nhật danh mục
        public ActionResult UpdateCategory(short id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            ViewBag.ParentCategoryId = new SelectList(_context.Categories.Where(c => c.CategoryId != id), "CategoryId", "CategoryName", category.ParentCategoryId);
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCategory(short id, Category category, bool IsActive)
        {
            if (id != category.CategoryId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật giá trị IsActive từ form
                    category.IsActive = IsActive;

                    // Nếu chọn "No Parent Category" thì gán ParentCategoryId = chính nó
                    if (category.ParentCategoryId == 0)
                    {
                        category.ParentCategoryId = category.CategoryId;
                    }

                    _context.Categories.Update(category);
                    _context.SaveChanges();
                    TempData["UpdateSuccess"] = "Cập nhật danh mục thành công!";
                    return RedirectToAction(nameof(ListCategory));
                }
                catch (Exception ex)
                {
                    TempData["UpdateError"] = "Có lỗi xảy ra khi cập nhật danh mục!";
                }
            }

            TempData["UpdateError"] = "Có lỗi xảy ra khi cập nhật danh mục!";
            return View(category);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCategory(short id)
        {
            var category = _context.Categories.Find(id);

            if (category == null)
            {
                return NotFound();
            }

            // Kiểm tra xem có danh mục con nào không (trừ chính nó)
            bool hasChildCategories = _context.Categories.Any(c => c.ParentCategoryId == id && c.CategoryId != id);

            if (hasChildCategories)
            {
                TempData["DeleteError"] = "Không thể xóa! Đang có danh mục khác lấy danh mục này làm cha.";
                return RedirectToAction("ListCategory", "Category");
            }

            _context.Categories.Remove(category);
            _context.SaveChanges();

            TempData["DeleteSuccess"] = "Danh mục đã được xóa thành công!";
            return RedirectToAction("ListCategory", "Category");
        }
    }
}
