using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Data.Models;

public partial class SubCategory
{
    [Key]
    [Required]
    public int SubCategoryId { get; set; }

    [Required(ErrorMessage = "Nhập đê")]
    [Display(Name = "Tên thưc mục con")]
    public string SubCategoryName { get; set; } = null!;

    [ValidateNever]
    [Display(Name = "Ảnh thư mục con")]
    public string SubCategoryImageUrl { get; set; } = null!;

    [ForeignKey("Category")]
    [Display(Name = "Chọn danh mục cha")]
    public int CategoryId { get; set; }

    [Required]
    [Display(Name = "Có kích hoạt không")]
    public bool IsActive { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [ValidateNever]
    public virtual Category Category { get; set; } = null!;
}
