namespace PurchaseOrderApi.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public List<OrderItem> OrderItems { get; set; } = [];
}
