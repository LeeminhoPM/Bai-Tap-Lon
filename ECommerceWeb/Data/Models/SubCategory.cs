using System;
using System.Collections.Generic;

namespace ECommerceWeb.Data.Models;

public partial class SubCategory
{
    public int SubCategoryId { get; set; }

    public string SubCategoryName { get; set; } = null!;

    public string SubCategoryImageUrl { get; set; } = null!;

    public int CategoryId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Category Category { get; set; } = null!;
}
