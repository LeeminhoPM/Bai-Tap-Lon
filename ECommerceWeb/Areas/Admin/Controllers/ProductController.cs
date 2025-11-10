using ECommerceWeb.Data;
using ECommerceWeb.Data.Models;
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

        public ProductController(EcommerceBtlContext db, IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
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
            return View(itemList.ToPagedList(pageIndex, pageSize));
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
                    ProductImageList = new List<ProductImage>(),
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
                        ProductImageList = productFromDb.ProductImages.ToList(),
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
            if (ModelState.IsValid)
            {
                obj.Product.CreatedDate = DateTime.Now;
                if (obj.Product.ProductId == 0)
                {
                    _db.Products.Add(obj.Product);
                }
                else
                {
                    _db.Products.Update(obj.Product);
                }
                _db.SaveChanges();

                // Địa chỉ của thư mục wwwroot
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                for (int i = 0; i < files.Count; i++)
                {
                    ProductImage productImage = new();
                    if (files[i] != null)
                    {
                        // Tên file mới = Id độc nhất + đuôi file (.jpg, .png, .jpeg, ...)
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(files[i].FileName);
                        // Địa chỉ của thư mục sẽ được copy ảnh sang
                        string categoryPath = Path.Combine(wwwRootPath, @"images\product");

                        // Copy ảnh vừa chọn vào thư mục
                        using (var fileStream = new FileStream(Path.Combine(categoryPath, fileName), FileMode.Create))
                        {
                            files[i].CopyTo(fileStream);
                        }

                        // Gán ImageUrl = đường dẫn của ảnh vừa copy
                        productImage.ImageUrl = @"\images\product\" + fileName;
                    }

                    productImage.ProductId = obj.Product.ProductId;
                    obj.Product.ProductImages.Add(productImage);
                }
                _db.SaveChanges();
                obj.ProductImageList = _db.Products.Include(p => p.ProductImages)
                    .FirstOrDefault(u => u.ProductId == obj.Product.ProductId)
                    .ProductImages.ToList();

                _db.ProductImages
                   .Where(x => x.ProductId == obj.Product.ProductId)
                   .ExecuteUpdate(s => s.SetProperty(p => p.DefaultImage, p => false));

                _db.ProductImages
                    .Where(x => x.ImageId == obj.ProductImageList[isDefault - 1].ImageId)
                    .ExecuteUpdate(s => s.SetProperty(p => p.DefaultImage, p => true));

                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                obj.CategoryList = new SelectList(_db.Categories, "CategoryId", "CategoryName");
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
                    if (!string.IsNullOrEmpty(image.ImageUrl))
                    {
                        // Kiểm tra xem có thư mục cũ đã chọn ở trong wwwroot chưa
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('\\'));

                        // Nếu có rồi thì xóa luôn cái cũ
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
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
                        if (!string.IsNullOrEmpty(image.ImageUrl))
                        {
                            // Kiểm tra xem có thư mục cũ đã chọn ở trong wwwroot chưa
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('\\'));
                            // Nếu có rồi thì xóa luôn cái cũ
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
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
                            if (!string.IsNullOrEmpty(image.ImageUrl))
                            {
                                // Kiểm tra xem có thư mục cũ đã chọn ở trong wwwroot chưa
                                var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('\\'));
                                // Nếu có rồi thì xóa luôn cái cũ
                                if (System.IO.File.Exists(oldImagePath))
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                            }
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
