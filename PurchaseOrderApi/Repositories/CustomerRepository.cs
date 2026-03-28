using Microsoft.EntityFrameworkCore;
using PurchaseOrderApi.Data;
using PurchaseOrderApi.Models;
using PurchaseOrderApi.Repositories.Interfaces;

namespace PurchaseOrderApi.Repositories;

public class CustomerRepository(AppDbContext context) : ICustomerRepository
{
    public async Task<IEnumerable<Customer>> GetAllAsync() =>
        await context.Customers.ToListAsync();

    public async Task<Customer?> GetByIdAsync(int id) =>
        await context.Customers.FindAsync(id);

    public async Task<Customer> CreateAsync(Customer customer)
    {
        context.Customers.Add(customer);
        await context.SaveChangesAsync();
        return customer;
    }
}
