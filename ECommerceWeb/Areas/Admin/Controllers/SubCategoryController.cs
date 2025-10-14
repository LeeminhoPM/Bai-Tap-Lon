using ECommerceWeb.Data;
using ECommerceWeb.Data.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public IActionResult Index()
        {
            List<SubCategory> items = _db.SubCategories.OrderByDescending(x => x.SubCategoryId).ToList();
            return View(items);
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
                    _db.SubCategories.Remove(subCategoryFromDb);
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
            return NotFound();
        }
    }
}
