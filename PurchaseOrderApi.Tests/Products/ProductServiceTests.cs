using AutoMapper;
using Moq;
using PurchaseOrderApi.DTOs.Products;
using PurchaseOrderApi.Mappings;
using PurchaseOrderApi.Models;
using PurchaseOrderApi.Repositories.Interfaces;
using PurchaseOrderApi.Services;

namespace PurchaseOrderApi.Tests.Products;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repoMock;
    private readonly IMapper _mapper;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repoMock = new Mock<IProductRepository>();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
            .CreateMapper();
        _service = new ProductService(_repoMock.Object, _mapper);
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", Price = 1500m, Stock = 10 },
            new() { Id = 2, Name = "Mouse",  Price = 25m,   Stock = 50 }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Laptop", result[0].Name);
        Assert.Equal("Mouse", result[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoProducts()
    {
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsProduct_WhenFound()
    {
        var product = new Product { Id = 1, Name = "Laptop", Price = 1500m, Stock = 10 };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Laptop", result.Name);
        Assert.Equal(1500m, result.Price);
        Assert.Equal(10, result.Stock);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Null(result);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ReturnsCreatedProduct()
    {
        var dto = new CreateProductDto { Name = "Keyboard", Price = 80m, Stock = 30 };
        var saved = new Product { Id = 3, Name = "Keyboard", Price = 80m, Stock = 30 };

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Product>())).ReturnsAsync(saved);

        var result = await _service.CreateAsync(dto);

        Assert.Equal(3, result.Id);
        Assert.Equal("Keyboard", result.Name);
        Assert.Equal(80m, result.Price);
        Assert.Equal(30, result.Stock);
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryOnce()
    {
        var dto = new CreateProductDto { Name = "Monitor", Price = 300m, Stock = 5 };
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Product>()))
                 .ReturnsAsync(new Product { Id = 4, Name = "Monitor", Price = 300m, Stock = 5 });

        await _service.CreateAsync(dto);

        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Product>()), Times.Once);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedProduct_WhenFound()
    {
        var existing = new Product { Id = 1, Name = "Laptop", Price = 1500m, Stock = 10 };
        var dto = new UpdateProductDto { Name = "Laptop Pro", Price = 1800m, Stock = 8 };
        var updated = new Product { Id = 1, Name = "Laptop Pro", Price = 1800m, Stock = 8 };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>())).ReturnsAsync(updated);

        var result = await _service.UpdateAsync(1, dto);

        Assert.NotNull(result);
        Assert.Equal("Laptop Pro", result.Name);
        Assert.Equal(1800m, result.Price);
        Assert.Equal(8, result.Stock);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _service.UpdateAsync(99, new UpdateProductDto());

        Assert.Null(result);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenFound()
    {
        var product = new Product { Id = 1, Name = "Laptop", Price = 1500m, Stock = 10 };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _service.DeleteAsync(1);

        Assert.True(result);
        _repoMock.Verify(r => r.DeleteAsync(product), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _service.DeleteAsync(99);

        Assert.False(result);
        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<Product>()), Times.Never);
    }
}
