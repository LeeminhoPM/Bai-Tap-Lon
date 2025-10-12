using ECommerceWeb.Data;
using Microsoft.AspNetCore.Mvc;
using ECommerceWeb.Data.Models;

namespace ECommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private EcommerceBtlContext _db;

        public CategoryController(EcommerceBtlContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            List<Category> itemList = _db.Categories.ToList();
            return View(itemList);
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Category obj)
        {
            if (ModelState.IsValid)
            {
                obj.CreatedDate = DateTime.Now;
                _db.Categories.Add(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id != null)
            {
                Category? categoryFromDb = _db.Categories.FirstOrDefault(u => u.CategoryId == id);
                if (categoryFromDb != null)
                {
                    return View(categoryFromDb);
                }
                return NotFound();
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                obj.CreatedDate = DateTime.Now;
                _db.Categories.Update(obj);
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id != null)
            {
                Category? categoryFromDb = _db.Categories.FirstOrDefault(u => u.CategoryId == id);
                if (categoryFromDb != null)
                {
                    _db.Categories.Remove(categoryFromDb);
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
            return NotFound();
        }
    }
}
