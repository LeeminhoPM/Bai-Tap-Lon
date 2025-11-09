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

        public IActionResult Index(int? page)
        {
            var pageSize = 10;
            if (page == null)
            {
                page = 1;
            }
            var pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;
            IEnumerable<Product> itemList = _db.Products.OrderByDescending(x => x.ProductId);
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
    }
}
