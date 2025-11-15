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
    }
}
