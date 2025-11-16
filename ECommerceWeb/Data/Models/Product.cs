using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Data.Models;

public partial class Product
{
    [Key]
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Nhập tên sản phẩm")]
    [DisplayName("Tên sản phẩm")]
    [MaxLength(100)]
    public string ProductName { get; set; } = null!;

    [DisplayName("Mô tả sản phẩm")]
    [MaxLength(200)]
    public string? ShortDescription { get; set; }

    // Dùng [Column(TypeName = "nvarchar(MAX)")] nếu bạn lo ngại về việc EF Core không tạo đúng nvarchar(max)
    [DisplayName("Chi tiết sản phẩm")]
    public string? LongDescription { get; set; }

    [DisplayName("Chi tiết thêm sản phẩm")]
    public string? AdditionalDescription { get; set; }

    [Required(ErrorMessage = "Nhập giá sản phẩm")]
    [DisplayName("Giá sản phẩm")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Nhập số lượng sản phẩm")]
    // Sửa lỗi chính tả: "Só lượng sản phẩm" -> "Số lượng sản phẩm"
    [DisplayName("Số lượng sản phẩm")]
    public int Quantity { get; set; }

    [DisplayName("Kích thước sản phẩm")]
    [MaxLength(30)]
    [ValidateNever]
    public string? Size { get; set; }

    [DisplayName("Màu sắc sản phẩm")]
    [MaxLength(50)]
    [ValidateNever]
    public string? Color { get; set; }

    [DisplayName("Tên công ty sản xuất")]
    [MaxLength(100)]
    public string? CompanyName { get; set; }

    [Required(ErrorMessage = "Chọn mã danh mục")]
    [DisplayName("Danh mục")]
    public int CategoryId { get; set; }

    [ValidateNever]
    [DisplayName("Danh mục con")]
    public int SubCategoryId { get; set; }

    [DisplayName("Đã bán")]
    [ValidateNever]
    public int? Sold { get; set; }

    [Required(ErrorMessage = "Chọn có thể custom riêng không")]
    [DisplayName("Có thể custom riêng")]
    public bool IsCustomized { get; set; }

    [Required(ErrorMessage = "Chọn trạng thái hiển thị")]
    [DisplayName("Hiển thị")]
    public bool IsActive { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [ValidateNever]
    [ForeignKey("CategoryId")]
    public virtual Category Category { get; set; } = null!;

    // Đảm bảo rằng Category và SubCategory (nếu có) cũng có các trường Tiếng Việt 
    // được cấu hình Unicode trong DB.

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
}