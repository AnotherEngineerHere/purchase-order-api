using PurchaseOrderApi.Models;

namespace PurchaseOrderApi.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(int id);
    Task<Order> CreateAsync(Order order);
}
