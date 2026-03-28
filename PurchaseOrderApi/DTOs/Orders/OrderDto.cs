namespace PurchaseOrderApi.DTOs.Orders;

public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal Total { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = [];
}
