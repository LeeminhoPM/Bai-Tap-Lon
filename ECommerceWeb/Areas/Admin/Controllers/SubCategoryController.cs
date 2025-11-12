using ECommerceWeb.Data;
using ECommerceWeb.Data.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.IO;

namespace ECommerceWeb.Areas.Admin.Controllers
{
    // --- ViewModel ---
    public class PagedSubCategoryViewModel
    {
        public List<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public string? SearchText { get; set; }
    }

    // --- Controller ---
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

        // --- 1. Index (Phân trang thủ công Async) ---
        public async Task<IActionResult> Index(string searchText, int? page)
        {
            var pageSize = 10;
            var pageIndex = page.HasValue && page.Value > 0 ? page.Value : 1;

            IQueryable<SubCategory> query = _db.SubCategories.Include(u => u.Category)
                                                             .OrderByDescending(x => x.SubCategoryId);

            if (!string.IsNullOrEmpty(searchText))
            {
                string search = searchText.ToLower();
                query = query.Where(x => x.SubCategoryName.ToLower().Contains(search) ||
                                         x.SubCategoryId.ToString().ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();

            var items = await query.Skip((pageIndex - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            var viewModel = new PagedSubCategoryViewModel
            {
                SubCategories = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                SearchText = searchText
            };

            return View(viewModel);
        }

        // --- 2. Upsert GET (Async) ---
        public async Task<IActionResult> Upsert(int? id)
        {
            ViewBag.CategoryOptions = new SelectList(await _db.Categories.ToListAsync(), "CategoryId", "CategoryName");

            if (id == null || id == 0)
            {
                return View(new SubCategory());
            }

            SubCategory? subCategoryFromDb = await _db.SubCategories.FirstOrDefaultAsync(u => u.SubCategoryId == id);

            if (subCategoryFromDb != null)
            {
                return View(subCategoryFromDb);
            }

            return NotFound();
        }

        // --- 3. Upsert POST (Async/File Handling) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(SubCategory obj, IFormFile? file)
        {
            if (obj.SubCategoryId == 0 && file == null)
            {
                ModelState.AddModelError("file", "Vui lòng chọn ảnh cho Danh mục con.");
            }

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                string imageFolder = @"images\subCategory";
                string categoryPath = Path.Combine(wwwRootPath, imageFolder);

                if (file != null)
                {
                    if (!System.IO.Directory.Exists(categoryPath))
                    {
                        System.IO.Directory.CreateDirectory(categoryPath);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                    if (!string.IsNullOrEmpty(obj.SubCategoryImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.SubCategoryImageUrl.TrimStart('\\', '/'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            await Task.Run(() => System.IO.File.Delete(oldImagePath));
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(categoryPath, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    obj.SubCategoryImageUrl = $"/{imageFolder.Replace('\\', '/')}/{fileName}";
                }

                if (obj.SubCategoryId == 0)
                {
                    obj.CreatedDate = DateTime.Now;
                    _db.SubCategories.Add(obj);
                }
                else
                {
                    _db.SubCategories.Update(obj);
                }

                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryOptions = new SelectList(await _db.Categories.ToListAsync(), "CategoryId", "CategoryName");
            return View(obj);
        }

        // --- 4. Delete (Async) ---
        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return Json(new { success = false, message = "ID không hợp lệ" });

            SubCategory? subCategoryFromDb = await _db.SubCategories.FirstOrDefaultAsync(u => u.SubCategoryId == id);

            if (subCategoryFromDb == null)
            {
                return Json(new { success = false, message = "Không tìm thấy Danh mục con" });
            }

            if (!string.IsNullOrEmpty(subCategoryFromDb.SubCategoryImageUrl))
            {
                string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, subCategoryFromDb.SubCategoryImageUrl.TrimStart('\\', '/'));

                if (System.IO.File.Exists(oldImagePath))
                {
                    await Task.Run(() => System.IO.File.Delete(oldImagePath));
                }
            }

            _db.SubCategories.Remove(subCategoryFromDb);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa thành công" });
        }

        // --- 5. IsActive (Async) ---
        [HttpPost]
        public async Task<IActionResult> IsActive(int? id)
        {
            if (id == null) return Json(new { success = false });

            SubCategory? subCategoryFromDb = await _db.SubCategories.FirstOrDefaultAsync(u => u.SubCategoryId == id);

            if (subCategoryFromDb != null)
            {
                subCategoryFromDb.IsActive = !subCategoryFromDb.IsActive;
                _db.Entry(subCategoryFromDb).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return Json(new { success = true, IsActive = subCategoryFromDb.IsActive });
            }
            return Json(new { success = false });
        }

        // --- 6. DeleteAll (Async/Truy vấn tối ưu) ---
        [HttpPost]
        public async Task<IActionResult> DeleteAll(string ids)
        {
            if (string.IsNullOrEmpty(ids))
            {
                return Json(new { success = false });
            }

            var itemIds = ids.Split(',')
                                .Select(s => int.TryParse(s, out int id) ? (int?)id : null)
                                .Where(id => id.HasValue)
                                .Select(id => id.Value)
                                .ToList();

            if (!itemIds.Any())
            {
                return Json(new { success = false, message = "Không có ID hợp lệ nào được cung cấp." });
            }

            var subCategoriesToDelete = await _db.SubCategories.Where(u => itemIds.Contains(u.SubCategoryId)).ToListAsync();

            foreach (var subCategory in subCategoriesToDelete)
            {
                if (!string.IsNullOrEmpty(subCategory.SubCategoryImageUrl))
                {
                    string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, subCategory.SubCategoryImageUrl.TrimStart('\\', '/'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        await Task.Run(() => System.IO.File.Delete(oldImagePath));
                    }
                }
                _db.SubCategories.Remove(subCategory);
            }

            await _db.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}