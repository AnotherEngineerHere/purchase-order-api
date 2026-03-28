using Microsoft.EntityFrameworkCore;
using PurchaseOrderApi.Data;
using PurchaseOrderApi.Models;
using PurchaseOrderApi.Repositories.Interfaces;

namespace PurchaseOrderApi.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<IEnumerable<Order>> GetAllAsync() =>
        await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .ToListAsync();

    public async Task<Order?> GetByIdAsync(int id) =>
        await context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order> CreateAsync(Order order)
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        return order;
    }
}
