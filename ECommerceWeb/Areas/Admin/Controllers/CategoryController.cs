using ECommerceWeb.Data;
using Microsoft.AspNetCore.Mvc;
using ECommerceWeb.Data.Models;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using System.Formats.Tar;
using X.PagedList.Extensions;
using ECommerceWeb.Function;

namespace ECommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly EcommerceBtlContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private ImageHandler imageHandler;

        public CategoryController(EcommerceBtlContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            imageHandler = new ImageHandler(_webHostEnvironment);
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
            return View(itemList.ToPagedList(pageIndex, pageSize));
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
                if (file != null)
                {
                    obj.CategoryImageUrl = imageHandler.SaveImage(file, @"images\product\", obj.CategoryImageUrl);
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
                    imageHandler.DeleteImage(categoryFromDb.CategoryImageUrl);
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
                        imageHandler.DeleteImage(categoryFromDb!.CategoryImageUrl);
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
