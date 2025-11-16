using ECommerceWeb.Data;
using ECommerceWeb.Data.Models;
using ECommerceWeb.Function;
using ECommerceWeb.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using X.PagedList.Extensions;

namespace ECommerceWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly EcommerceBtlContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private ImageHandler imageHandler;

        public ProductController(EcommerceBtlContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            imageHandler = new ImageHandler(webHostEnvironment);
        }

        public IActionResult Index(string searchText, int? page)
        {
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            IEnumerable<Product> itemList = _db.Products.Include(p => p.ProductImages).OrderByDescending(x => x.ProductId);
            if (!string.IsNullOrEmpty(searchText))
            {
                itemList = itemList.Where(x => x.ProductName.ToLower().Contains(searchText.ToLower()) || x.ProductId.ToString().ToLower().Contains(searchText.ToLower()));
            }
            var model = itemList.ToPagedList(pageIndex, pageSize);

            // Nếu là AJAX request → trả về PartialView
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ProductTable", model);
            }

            return View(model);
        }

        public JsonResult GetSubCategoryById(int id)
        {
            var SubCategoryList = _db.SubCategories.Where(x => x.CategoryId == id).ToList();
            return Json(SubCategoryList);

        }

        public IActionResult Upsert(int? id)
        {
            SelectList categoryList = new SelectList(_db.Categories, "CategoryId", "CategoryName");
            if (id == null || id == 0)
            {
                ProductVM productVM = new()
                {
                    CategoryList = categoryList,
                    Product = new Product(),
                };
                return View(productVM);
            }
            else
            {
                Product? productFromDb = _db.Products.Include(p => p.ProductImages).FirstOrDefault(u => u.ProductId == id);
                if (productFromDb != null)
                {
                    ProductVM productVM = new()
                    {
                        CategoryList = categoryList,
                        Product = productFromDb,
                    };
                    return View(productVM);
                }
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductVM obj, List<IFormFile> files, int isDefault)
        {
            // Đảm bảo Product.ProductImages không phải là null
            if (obj.Product.ProductImages == null)
            {
                obj.Product.ProductImages = new List<ProductImage>();
            }

            if (ModelState.IsValid)
            {
                // 1. Thêm hoặc Cập nhật Product
                obj.Product.CreatedDate = DateTime.Now;
                if (obj.Product.ProductId == 0)
                {
                    _db.Products.Add(obj.Product);
                }
                else
                {
                    _db.Products.Update(obj.Product);
                }
                _db.SaveChanges(); // Lưu để có ProductId cho các bước tiếp theo

                string wwwRootPath = _webHostEnvironment.WebRootPath;

                // Danh sách tạm thời để lưu ProductImage mới được tạo
                var newProductImages = new List<ProductImage>();

                // 2. Xử lý và Lưu từng File ảnh
                for (int i = 0; i < files.Count; i++)
                {
                    IFormFile file = files[i];
                    if (file != null && file.Length > 0)
                    {
                        ProductImage productImage = new();
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string categoryPath = Path.Combine(wwwRootPath, @"images\products");

                        // Copy ảnh vừa chọn vào thư mục
                        using (var fileStream = new FileStream(Path.Combine(categoryPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        // Gán ImageUrl và ProductId
                        productImage.ImageUrl = @"\images\products\" + fileName;
                        productImage.ProductId = obj.Product.ProductId;

                        // Thêm vào DBContext
                        _db.ProductImages.Add(productImage);
                        // Thêm vào danh sách tạm thời để xác định DefaultImage sau này
                        newProductImages.Add(productImage);
                    }
                }
                _db.SaveChanges(); // Lưu tất cả ProductImage mới để có ImageId

                // 3. Cập nhật ảnh mặc định (DefaultImage)

                // Kiểm tra xem có ảnh nào mới được thêm không và isDefault có hợp lệ không
                if (newProductImages.Count > 0)
                {
                    // Đặt tất cả ảnh hiện tại của sản phẩm về DefaultImage = false
                    _db.ProductImages
                       .Where(x => x.ProductId == obj.Product.ProductId)
                       .ExecuteUpdate(s => s.SetProperty(p => p.DefaultImage, p => false));

                    // Xác định index ảnh mặc định. isDefault là 1-based, index là 0-based.
                    // Đảm bảo index nằm trong phạm vi [1, newProductImages.Count]
                    int defaultImageIndex = Math.Max(1, Math.Min(isDefault, newProductImages.Count));

                    // Lấy ImageId của ảnh được chọn làm mặc định từ danh sách ảnh mới đã được lưu (và có ImageId)
                    int defaultImageIdToSet = newProductImages[defaultImageIndex - 1].ImageId;

                    // Cập nhật ảnh mặc định
                    _db.ProductImages
                        .Where(x => x.ImageId == defaultImageIdToSet)
                        .ExecuteUpdate(s => s.SetProperty(p => p.DefaultImage, p => true));

                    // NOTE: Đã loại bỏ _db.SaveChanges() cuối cùng vì ExecuteUpdate đã thực thi trên DB
                }
                else if (obj.Product.ProductId != 0)
                {
                    // Trường hợp cập nhật nhưng không có ảnh mới, có thể cần logic để giữ ảnh mặc định cũ
                    // (hoặc không làm gì nếu ảnh mặc định đã được thiết lập trước đó).
                }
                return RedirectToAction("Index");
            }
            else
            {
                // ... (Giữ nguyên logic lỗi)
                obj.CategoryList = new SelectList(_db.Categories, "CategoryId", "CategoryName");
                // Bạn có thể cần load lại ProductImageList nếu đang cập nhật
                return View(obj);
            }
        }

        public async Task<IActionResult> DeleteProductImage(int? id)
        {
            if (id != null)
            {
                ProductImage? image = _db.ProductImages.FirstOrDefault(u => u.ImageId == id);
                if (image != null)
                {
                    imageHandler.DeleteImage(image.ImageUrl);
                    _db.ProductImages.Remove(image);
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
            return NotFound();
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id != null)
            {
                Product? productFromDb = _db.Products.Include(x => x.ProductImages).FirstOrDefault(u => u.ProductId == id);
                if (productFromDb != null)
                {
                    foreach (var image in productFromDb.ProductImages)
                    {
                        imageHandler.DeleteImage(image.ImageUrl);
                    }
                    _db.Products.Remove(productFromDb);
                    _db.SaveChanges();
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
            return NotFound();
        }

        public async Task<IActionResult> DeleteAll(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var items = ids.Split(',');
                if (items != null && items.Any())
                {
                    foreach (var item in items)
                    {
                        Product? productFromDb = _db.Products.Include(u => u.ProductImages).FirstOrDefault(u => u.ProductId == int.Parse(item));
                        foreach (var image in productFromDb.ProductImages)
                        {
                            imageHandler.DeleteImage(image.ImageUrl);
                        }
                        _db.Products.Remove(productFromDb);
                        _db.SaveChanges();
                    }
                }
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }
    }
}
