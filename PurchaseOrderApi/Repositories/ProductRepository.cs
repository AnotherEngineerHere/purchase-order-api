using Microsoft.EntityFrameworkCore;
using PurchaseOrderApi.Data;
using PurchaseOrderApi.Models;
using PurchaseOrderApi.Repositories.Interfaces;

namespace PurchaseOrderApi.Repositories;

public class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<IEnumerable<Product>> GetAllAsync() =>
        await context.Products.ToListAsync();

    public async Task<Product?> GetByIdAsync(int id) =>
        await context.Products.FindAsync(id);

    public async Task<Product> CreateAsync(Product product)
    {
        context.Products.Add(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        context.Products.Update(product);
        await context.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(Product product)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync();
    }
}
