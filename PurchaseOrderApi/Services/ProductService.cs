using AutoMapper;
using PurchaseOrderApi.DTOs.Products;
using PurchaseOrderApi.Models;
using PurchaseOrderApi.Repositories.Interfaces;
using PurchaseOrderApi.Services.Interfaces;

namespace PurchaseOrderApi.Services;

public class ProductService(IProductRepository repository, IMapper mapper) : IProductService
{
    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        var products = await repository.GetAllAsync();
        return mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await repository.GetByIdAsync(id);
        return product is null ? null : mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        var product = mapper.Map<Product>(dto);
        var created = await repository.CreateAsync(product);
        return mapper.Map<ProductDto>(created);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var product = await repository.GetByIdAsync(id);
        if (product is null) return null;

        mapper.Map(dto, product);
        var updated = await repository.UpdateAsync(product);
        return mapper.Map<ProductDto>(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await repository.GetByIdAsync(id);
        if (product is null) return false;

        await repository.DeleteAsync(product);
        return true;
    }
}
