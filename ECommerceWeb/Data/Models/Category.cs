using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerceWeb.Data.Models;

public partial class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Mục này không được để trống")]
    [MaxLength(100)]
    [Display(Name = "Tên danh mục")]
    public string CategoryName { get; set; } = null!;

    [Required(ErrorMessage = "Mục này không được để trống")]
    [Display(Name = "Ảnh danh mục")]
    public string CategoryImageUrl { get; set; } = null!;

    [Required(ErrorMessage = "Mục này không được để trống")]
    [Display(Name = "Kích hoạt luôn không")]
    public bool IsActive { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    public virtual ICollection<Product> Products { get; set; }

    public virtual ICollection<SubCategory> SubCategories { get; set; }

    public Category()
    {
        Products = new HashSet<Product>();
        SubCategories = new HashSet<SubCategory>();
    }
}
