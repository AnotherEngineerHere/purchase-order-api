using AutoMapper;
using PurchaseOrderApi.DTOs.Customers;
using PurchaseOrderApi.Models;
using PurchaseOrderApi.Repositories.Interfaces;
using PurchaseOrderApi.Services.Interfaces;

namespace PurchaseOrderApi.Services;

public class CustomerService(ICustomerRepository repository, IMapper mapper) : ICustomerService
{
    public async Task<IEnumerable<CustomerDto>> GetAllAsync()
    {
        var customers = await repository.GetAllAsync();
        return mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<CustomerDto?> GetByIdAsync(int id)
    {
        var customer = await repository.GetByIdAsync(id);
        return customer is null ? null : mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto)
    {
        var customer = mapper.Map<Customer>(dto);
        var created = await repository.CreateAsync(customer);
        return mapper.Map<CustomerDto>(created);
    }
}
