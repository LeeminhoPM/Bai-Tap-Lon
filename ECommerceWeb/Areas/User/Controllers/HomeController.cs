using ECommerceWeb.Data;
using ECommerceWeb.Data.Models;
using ECommerceWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
namespace ECommerceWeb.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EcommerceBtlContext _db;

        public HomeController(ILogger<HomeController> logger, EcommerceBtlContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _db.Products
                                        .Where(p => p.IsActive == true) // <--- DÒNG QUAN TRỌNG
                                        .Include(p => p.Category)
                                        .Include(p => p.ProductImages)
                                        .ToList();

            return View(productList);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Detail(int id)
        {
            var product = _db.Products
       .Include(p => p.ProductImages)
       .Include(p => p.Category)
       .FirstOrDefault(p => p.ProductId == id);

            return View(product);
        }
        public async Task<IActionResult> QuickView(int id)
        {
            // 1. Tải sản phẩm và ảnh từ database
            var product = await _db.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                // Trả về một thông báo lỗi nếu không tìm thấy sản phẩm
                return NotFound();
            }

            // 2. Trả về Partial View chứa nội dung Modal
            return PartialView("_QuickViewModalContent", product);
            // LƯU Ý: Bạn cần tạo file _QuickViewModalContent.cshtml
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
