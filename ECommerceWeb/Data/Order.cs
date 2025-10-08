using System;
using System.Collections.Generic;

namespace ECommerceWeb.Data;

public partial class Order
{
    public int OrderDetailsId { get; set; }

    public string? OrderNo { get; set; }

    public int ProductId { get; set; }

    public int? Quantity { get; set; }

    public int UserId { get; set; }

    public string? Status { get; set; }

    public int PaymentId { get; set; }

    public DateTime OrderDate { get; set; }

    public bool IsCancel { get; set; }

    public virtual Payment Payment { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
