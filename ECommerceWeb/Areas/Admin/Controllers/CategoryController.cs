using ECommerceWeb.Data;
using Microsoft.AspNetCore.Mvc;
using ECommerceWeb.Data.Models;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using System.Formats.Tar;
using X.PagedList.Extensions;

namespace ECommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly EcommerceBtlContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CategoryController(EcommerceBtlContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index(string searchText, int? page)
        {
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            IEnumerable<Category> itemList = _db.Categories.OrderByDescending(x => x.CategoryId);
            if (!string.IsNullOrEmpty(searchText))
            {
                itemList = itemList.Where(x => x.CategoryName.ToLower().Contains(searchText.ToLower()) || x.CategoryId.ToString().ToLower().Contains(searchText.ToLower()));
            }
            var model = itemList.ToPagedList(pageIndex, pageSize);

            // Nếu là AJAX request → trả về PartialView
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CategoryTable", model);
            }

            return View(model);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                return View(new Category());
            }
            else
            {
                Category? categoryFromDb = _db.Categories.FirstOrDefault(u => u.CategoryId == id);
                if (categoryFromDb != null)
                {
                    return View(categoryFromDb);
                }
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Category obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                // Địa chỉ của thư mục wwwroot
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    // Tên file mới = Id độc nhất + đuôi file (.jpg, .png, .jpeg, ...)
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    // Địa chỉ của thư mục sẽ được copy ảnh sang
                    string categoryPath = Path.Combine(wwwRootPath, @"images\category");

                    if (!string.IsNullOrEmpty(obj.CategoryImageUrl))
                    {
                        // Kiểm tra xem có thư mục cũ đã chọn ở trong wwwroot chưa
                        var oldImagePath = Path.Combine(wwwRootPath, obj.CategoryImageUrl.TrimStart('\\'));

                        // Nếu có rồi thì xóa luôn cái cũ
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Copy ảnh vừa chọn vào thư mục
                    using (var fileStream = new FileStream(Path.Combine(categoryPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    // Gán ImageUrl = đường dẫn của ảnh vừa copy
                    obj.CategoryImageUrl = @"\images\category\" + fileName;
                }

                obj.CreatedDate = DateTime.Now;
                if (obj.CategoryId == 0)
                {
                    _db.Categories.Add(obj);
                }
                else
                {
                    _db.Categories.Update(obj);
                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        public IActionResult Delete(int? id)
        {
            if (id != null)
            {
                Category? categoryFromDb = _db.Categories.Include(u => u.SubCategories).FirstOrDefault(u => u.CategoryId == id);
                if (categoryFromDb != null)
                {
                    if (!string.IsNullOrEmpty(categoryFromDb.CategoryImageUrl))
                    {
                        // Kiểm tra xem có thư mục cũ đã chọn ở trong wwwroot chưa
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, categoryFromDb.CategoryImageUrl.TrimStart('\\'));

                        // Nếu có rồi thì xóa luôn cái cũ
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    _db.Categories.Remove(categoryFromDb);
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
            return NotFound();
        }

        public IActionResult IsActive(int? id)
        {
            if (id != null)
            {
                Category? categoryFromDb = _db.Categories.FirstOrDefault(u => u.CategoryId == id);
                if (categoryFromDb != null)
                {
                    categoryFromDb.IsActive = !categoryFromDb.IsActive;
                    _db.Entry(categoryFromDb).State = EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { success = true, IsActive = categoryFromDb.IsActive });
                }
                return Json(new { success = false });
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = ids.Split(',');
                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        Category? categoryFromDb = _db.Categories.Include(u => u.SubCategories).FirstOrDefault(u => u.CategoryId == int.Parse(item));
                        if (!string.IsNullOrEmpty(categoryFromDb.CategoryImageUrl))
                        {
                            // Kiểm tra xem có thư mục cũ đã chọn ở trong wwwroot chưa
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, categoryFromDb.CategoryImageUrl.TrimStart('\\'));

                            // Nếu có rồi thì xóa luôn cái cũ
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        _db.Categories.Remove(categoryFromDb);
                        _db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        

    }
}
