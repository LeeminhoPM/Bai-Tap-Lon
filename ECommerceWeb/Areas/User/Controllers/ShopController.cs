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

        public IActionResult Index(int? id)
        {
            var productList = new List<Product>();
            if (id == null || id == 0)
            {
                productList = _db.Products.ToList();
            }
            else
            {
                productList = _db.Products.Where(p => p.CategoryId == id).ToList();
            }
            return View(productList);
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
