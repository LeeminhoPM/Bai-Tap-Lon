using ECommerceWeb.Data;
using ECommerceWeb.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWeb.Areas.User.Controllers
{
    [Area("User")]
    public class ShopController : Controller
    {
        private readonly EcommerceBtlContext _db;

        public ShopController(EcommerceBtlContext context) => _db = context;

        public async Task<IActionResult> Index()
        {
            var productList = _db.Products.Include(p => p.ProductImages).ToList();
            return View(productList);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var product = await _db.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            return View(product);
        }

        public IActionResult Detail(int id)
        {
            var product = _db.Products
        // 1. Phải INCLUDE ProductImages và Category
        .Include(p => p.ProductImages.OrderByDescending(img => img.DefaultImage)) // 🎯 Sắp xếp để ảnh DefaultImage = true lên đầu
        .Include(p => p.Category)
        .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}
