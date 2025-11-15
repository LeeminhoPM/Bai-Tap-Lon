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
                    productImage.ImageUrl = imageHandler.SaveImage(files[i], "images/product/", productImage.ImageUrl);
                    productImage.ProductId = obj.Product.ProductId;
                    obj.Product.ProductImages.Add(productImage);
                }
                _db.SaveChanges();
                _db.ProductImages.ToList().ForEach(img =>
                {
                    if (img.ProductId == obj.Product.ProductId)
                    {
                        if (isDefault == img.ImageId)
                        {
                            img.DefaultImage = true;
                        }
                        else
                        {
                            img.DefaultImage = false;
                        }
                    }
                });
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
