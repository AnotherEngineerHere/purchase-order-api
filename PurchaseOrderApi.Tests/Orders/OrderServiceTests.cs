using AutoMapper;
using Moq;
using PurchaseOrderApi.DTOs.Orders;
using PurchaseOrderApi.Mappings;
using PurchaseOrderApi.Models;
using PurchaseOrderApi.Repositories.Interfaces;
using PurchaseOrderApi.Services;

namespace PurchaseOrderApi.Tests.Orders;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository>    _orderRepoMock;
    private readonly Mock<IProductRepository>  _productRepoMock;
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly IMapper _mapper;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _orderRepoMock    = new Mock<IOrderRepository>();
        _productRepoMock  = new Mock<IProductRepository>();
        _customerRepoMock = new Mock<ICustomerRepository>();
        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>())
            .CreateMapper();

        _service = new OrderService(
            _orderRepoMock.Object,
            _productRepoMock.Object,
            _customerRepoMock.Object,
            _mapper);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Order BuildOrder(int id, Customer customer, List<OrderItem> items)
    {
        var total = items.Sum(i => i.Quantity * i.UnitPrice);
        return new Order
        {
            Id         = id,
            CustomerId = customer.Id,
            Customer   = customer,
            Total      = total,
            CreatedAt  = DateTime.UtcNow,
            OrderItems = items
        };
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrders()
    {
        var customer = new Customer { Id = 1, FullName = "Alice", Email = "alice@example.com" };
        var product  = new Product  { Id = 1, Name = "Laptop", Price = 1500m, Stock = 10 };
        var items    = new List<OrderItem>
        {
            new() { Id = 1, ProductId = 1, Product = product, Quantity = 2, UnitPrice = 1500m }
        };
        var orders = new List<Order> { BuildOrder(1, customer, items) };
        _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(orders);

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Single(result);
        Assert.Equal(1, result[0].CustomerId);
        Assert.Equal(3000m, result[0].Total);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoOrders()
    {
        _orderRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync([]);

        var result = await _service.GetAllAsync();

        Assert.Empty(result);
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ReturnsOrder_WhenFound()
    {
        var customer = new Customer { Id = 1, FullName = "Alice", Email = "alice@example.com" };
        var product  = new Product  { Id = 1, Name = "Laptop", Price = 1500m, Stock = 10 };
        var items    = new List<OrderItem>
        {
            new() { Id = 1, ProductId = 1, Product = product, Quantity = 1, UnitPrice = 1500m }
        };
        var order = BuildOrder(1, customer, items);
        _orderRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _service.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1500m, result.Total);
        Assert.Single(result.OrderItems);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _orderRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Order?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Null(result);
    }

    // ── Create — validations ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ThrowsKeyNotFoundException_WhenCustomerNotFound()
    {
        _customerRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Customer?)null);

        var dto = new CreateOrderDto
        {
            CustomerId = 99,
            OrderItems = [new CreateOrderItemDto { ProductId = 1, Quantity = 1 }]
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ThrowsKeyNotFoundException_WhenProductNotFound()
    {
        var customer = new Customer { Id = 1, FullName = "Alice", Email = "alice@example.com" };
        _customerRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            OrderItems = [new CreateOrderItemDto { ProductId = 99, Quantity = 1 }]
        };

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CreateAsync(dto));
    }

    [Fact]
    public async Task CreateAsync_ThrowsInvalidOperationException_WhenInsufficientStock()
    {
        var customer = new Customer { Id = 1, FullName = "Alice", Email = "alice@example.com" };
        var product  = new Product  { Id = 1, Name = "Laptop", Price = 1500m, Stock = 2 };

        _customerRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            OrderItems = [new CreateOrderItemDto { ProductId = 1, Quantity = 5 }] // 5 > stock 2
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));
    }

    // ── Create — happy path ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_CalculatesTotalCorrectly()
    {
        var customer = new Customer { Id = 1, FullName = "Alice", Email = "alice@example.com" };
        var product1 = new Product  { Id = 1, Name = "Laptop", Price = 1500m, Stock = 10 };
        var product2 = new Product  { Id = 2, Name = "Mouse",  Price = 25m,   Stock = 50 };

        _customerRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product1);
        _productRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(product2);
        _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
                        .ReturnsAsync((Product p) => p);

        Order? savedOrder = null;
        _orderRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Order>()))
            .Callback<Order>(o => savedOrder = o)
            .ReturnsAsync((Order o) => o);

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(() =>
            {
                savedOrder!.Customer   = customer;
                savedOrder.OrderItems  = savedOrder.OrderItems.Select(oi =>
                {
                    oi.Product = oi.ProductId == 1 ? product1 : product2;
                    return oi;
                }).ToList();
                return savedOrder;
            });

        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            OrderItems =
            [
                new CreateOrderItemDto { ProductId = 1, Quantity = 2 }, // 2 × 1500 = 3000
                new CreateOrderItemDto { ProductId = 2, Quantity = 4 }  // 4 × 25   = 100
            ]
        };

        var result = await _service.CreateAsync(dto);

        // Total = 3000 + 100 = 3100
        Assert.Equal(3100m, result.Total);
        Assert.Equal(2, result.OrderItems.Count);
    }

    [Fact]
    public async Task CreateAsync_ReducesProductStock()
    {
        var customer = new Customer { Id = 1, FullName = "Alice", Email = "alice@example.com" };
        var product  = new Product  { Id = 1, Name = "Laptop", Price = 1500m, Stock = 10 };

        _customerRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Product>()))
                        .ReturnsAsync((Product p) => p);

        Order? savedOrder = null;
        _orderRepoMock
            .Setup(r => r.CreateAsync(It.IsAny<Order>()))
            .Callback<Order>(o => savedOrder = o)
            .ReturnsAsync((Order o) => o);

        _orderRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(() =>
            {
                savedOrder!.Customer = customer;
                savedOrder.OrderItems = savedOrder.OrderItems.Select(oi =>
                {
                    oi.Product = product;
                    return oi;
                }).ToList();
                return savedOrder;
            });

        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            OrderItems = [new CreateOrderItemDto { ProductId = 1, Quantity = 3 }]
        };

        await _service.CreateAsync(dto);

        // Stock original 10 − 3 = 7
        Assert.Equal(7, product.Stock);
        _productRepoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Stock == 7)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DoesNotReduceStock_WhenValidationFails()
    {
        var customer = new Customer { Id = 1, FullName = "Alice", Email = "alice@example.com" };
        var product  = new Product  { Id = 1, Name = "Laptop", Price = 1500m, Stock = 2 };

        _customerRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(customer);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var dto = new CreateOrderDto
        {
            CustomerId = 1,
            OrderItems = [new CreateOrderItemDto { ProductId = 1, Quantity = 10 }]
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto));

        // Stock debe mantenerse intacto
        Assert.Equal(2, product.Stock);
        _productRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }
}
