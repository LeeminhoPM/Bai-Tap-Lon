using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceWeb.Data.Models;

public partial class ProductImage
{
    [Key]
    public int ImageId { get; set; }

    [Required]
    public string? ImageUrl { get; set; }
    
    [Required]
    public int ProductId { get; set; }

    [Required]
    [DisplayName("Chọn làm ảnh đại diện")]
    public bool DefaultImage { get; set; }

    [ValidateNever]
    [ForeignKey("ProductId")]
    public virtual Product Product { get; set; } = null!;
}
