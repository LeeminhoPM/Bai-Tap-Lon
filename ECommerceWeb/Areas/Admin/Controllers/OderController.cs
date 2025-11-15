using Microsoft.AspNetCore.Mvc;
using ECommerceWeb.Data;

namespace ECommerceWeb.Areas.Admin.Controllers
{
    public class OderController : Controller
    {

        private EcommerceBtlContext db = new EcommerceBtlContext();
        //public IActionResult Index(int? page)
        //{
        //    var items= db.Orders.OrderByDescending(x => x.CreatedDate).ToList();
        //    if(page==null) page=1;
            
        //    var pageNumber = page ?? 1;
        //    var pageSize = 15;

        //    return View(items.ToPagedList(pageNumber,pageSize));
        //}
    }
}
