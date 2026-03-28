using PurchaseOrderApi.DTOs.Customers;

namespace PurchaseOrderApi.Services.Interfaces;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(int id);
    Task<CustomerDto> CreateAsync(CreateCustomerDto dto);
}
