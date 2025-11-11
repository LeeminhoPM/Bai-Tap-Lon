using ECommerceWeb.Data;
using ECommerceWeb.Data.Models;
using Microsoft.AspNetCore.Mvc;

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
    }
}
