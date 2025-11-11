using ECommerceWeb.Data;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWeb.Areas.User.ViewComponents
{
    public class CategoryViewComponent : ViewComponent
    {
        private readonly EcommerceBtlContext _db;

        public CategoryViewComponent(EcommerceBtlContext context) => _db = context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = _db.Categories.ToList();
            return View(categories);
        }
    }
}
