using AutoMapper;
using Moq;
using PurchaseOrderApi.DTOs.Customers;
using PurchaseOrderApi.Mappings;
using PurchaseOrderApi.Models;
using PurchaseOrderApi.Repositories.Interfaces;
using PurchaseOrderApi.Services;

namespace PurchaseOrderApi.Tests.Customers;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _repoMock;
    private readonly IMapper _mapper;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _repoMock = new Mock<ICustomerRepository>();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
            .CreateMapper();
        _service = new CustomerService(_repoMock.Object, _mapper);
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllCustomers()
    {
        var customers = new List<Customer>
        {
            new() { Id = 1, FullName = "Alice Smith", Email = "alice@example.com" },
            new() { Id = 2, FullName = "Bob Jones",  Email = "bob@example.com" }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(customers);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alice Smith", result[0].FullName);
        Assert.Equal("bob@example.com", result[1].Email);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoCustomers()
    {
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsCustomer_WhenFound()
    {
        var customer = new Customer { Id = 1, FullName = "Alice Smith", Email = "alice@example.com" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Alice Smith", result.FullName);
        Assert.Equal("alice@example.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Customer?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Null(result);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ReturnsCreatedCustomer()
    {
        var dto = new CreateCustomerDto { FullName = "Carol White", Email = "carol@example.com" };
        var saved = new Customer { Id = 3, FullName = "Carol White", Email = "carol@example.com" };

        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Customer>())).ReturnsAsync(saved);

        var result = await _service.CreateAsync(dto);

        Assert.Equal(3, result.Id);
        Assert.Equal("Carol White", result.FullName);
        Assert.Equal("carol@example.com", result.Email);
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryOnce()
    {
        var dto = new CreateCustomerDto { FullName = "Dan Brown", Email = "dan@example.com" };
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<Customer>()))
                 .ReturnsAsync(new Customer { Id = 4, FullName = "Dan Brown", Email = "dan@example.com" });

        await _service.CreateAsync(dto);

        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Customer>()), Times.Once);
    }
}
