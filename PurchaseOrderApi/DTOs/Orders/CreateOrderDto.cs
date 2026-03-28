using System.ComponentModel.DataAnnotations;

namespace PurchaseOrderApi.DTOs.Orders;

public class CreateOrderDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one order item is required.")]
    public List<CreateOrderItemDto> OrderItems { get; set; } = [];
}

public class CreateOrderItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}
