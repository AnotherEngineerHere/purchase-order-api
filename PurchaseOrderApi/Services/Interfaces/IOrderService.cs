using PurchaseOrderApi.DTOs.Orders;

namespace PurchaseOrderApi.Services.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderDto>> GetAllAsync();
    Task<OrderDto?> GetByIdAsync(int id);
    Task<OrderDto> CreateAsync(CreateOrderDto dto);
}
