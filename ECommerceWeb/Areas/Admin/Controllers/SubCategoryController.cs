using ECommerceWeb.Data;
using ECommerceWeb.Data.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace ECommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly EcommerceBtlContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SubCategoryController(EcommerceBtlContext db, IWebHostEnvironment webHostEnvironment)
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
            IEnumerable<SubCategory> itemList = _db.SubCategories.OrderByDescending(x => x.SubCategoryId);
            if (!string.IsNullOrEmpty(searchText))
            {
                itemList = itemList.Where(x => x.SubCategoryName.ToLower().Contains(searchText.ToLower()) || x.SubCategoryId.ToString().ToLower().Contains(searchText.ToLower()));
            }
            return View(itemList.ToPagedList(pageIndex, pageSize));
        }

        public IActionResult Upsert(int? id)
        {
            ViewBag.CategoryOptions = new SelectList(_db.Categories, "CategoryId", "CategoryName");
            if (id == null || id == 0)
            {
                return View(new SubCategory());
            }
            else
            {
                SubCategory? subCategoryFromDb = _db.SubCategories.FirstOrDefault(u => u.SubCategoryId == id);
                if (subCategoryFromDb != null)
                {
                    return View(subCategoryFromDb);
                }
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(SubCategory obj, IFormFile? file)
        {
            Console.WriteLine(ModelState.IsValid);
            if (ModelState.IsValid)
            {
                // Địa chỉ của thư mục wwwroot
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    // Tên file mới = Id độc nhất + đuôi file (.jpg, .png, .jpeg, ...)
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    // Địa chỉ của thư mục sẽ được copy ảnh sang
                    string categoryPath = Path.Combine(wwwRootPath, @"images\subCategory");

                    if (!string.IsNullOrEmpty(obj.SubCategoryImageUrl))
                    {
                        // Kiểm tra xem có thư mục cũ đã chọn ở trong wwwroot chưa
                        var oldImagePath = Path.Combine(wwwRootPath, obj.SubCategoryImageUrl.TrimStart('\\'));

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
                    obj.SubCategoryImageUrl = @"\images\subCategory\" + fileName;
                }

                obj.CreatedDate = DateTime.Now;
                if (obj.SubCategoryId == 0)
                {
                    _db.SubCategories.Add(obj);
                }
                else
                {
                    _db.SubCategories.Update(obj);
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
                SubCategory? subCategoryFromDb = _db.SubCategories.FirstOrDefault(u => u.SubCategoryId == id);
                if (subCategoryFromDb != null)
                {
                    if (!string.IsNullOrEmpty(subCategoryFromDb.SubCategoryImageUrl))
                    {
                        // Kiểm tra xem có thư mục cũ đã chọn ở trong wwwroot chưa
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, subCategoryFromDb.SubCategoryImageUrl.TrimStart('\\'));

                        // Nếu có rồi thì xóa luôn cái cũ
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    _db.SubCategories.Remove(subCategoryFromDb);
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
                SubCategory? subCategoryFromDb = _db.SubCategories.FirstOrDefault(u => u.SubCategoryId == id);
                if (subCategoryFromDb != null)
                {
                    subCategoryFromDb.IsActive = !subCategoryFromDb.IsActive;
                    _db.Entry(subCategoryFromDb).State = EntityState.Modified;
                    _db.SaveChanges();
                    return Json(new { success = true, IsActive = subCategoryFromDb.IsActive });
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
                        SubCategory? subCategoryFromDb = _db.SubCategories.FirstOrDefault(u => u.SubCategoryId == int.Parse(item));
                        if (!string.IsNullOrEmpty(subCategoryFromDb.SubCategoryImageUrl))
                        {
                            // Kiểm tra xem có thư mục cũ đã chọn ở trong wwwroot chưa
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, subCategoryFromDb.SubCategoryImageUrl.TrimStart('\\'));

                            // Nếu có rồi thì xóa luôn cái cũ
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        _db.SubCategories.Remove(subCategoryFromDb);
                        _db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
