using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using ECommerceWeb.Data.Models;

namespace ECommerceWeb.Data.ViewModels
{
    public class ProductViewModel
    {
        public Product product { get; set; }
        [DisplayName("Tải lên hình ảnh")]
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một ảnh sản phẩm")]
        public List<IFormFile> ImageFiles { get; set; }
    }
}
