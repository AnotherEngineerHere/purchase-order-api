using AutoMapper;
using PurchaseOrderApi.DTOs.Orders;
using PurchaseOrderApi.Models;
using PurchaseOrderApi.Repositories.Interfaces;
using PurchaseOrderApi.Services.Interfaces;

namespace PurchaseOrderApi.Services;

public class OrderService(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    ICustomerRepository customerRepository,
    IMapper mapper) : IOrderService
{
    public async Task<IEnumerable<OrderDto>> GetAllAsync()
    {
        var orders = await orderRepository.GetAllAsync();
        return mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto?> GetByIdAsync(int id)
    {
        var order = await orderRepository.GetByIdAsync(id);
        return order is null ? null : mapper.Map<OrderDto>(order);
    }

    public async Task<OrderDto> CreateAsync(CreateOrderDto dto)
    {
        var customer = await customerRepository.GetByIdAsync(dto.CustomerId)
            ?? throw new KeyNotFoundException($"Customer with id {dto.CustomerId} not found.");

        var orderItems = new List<OrderItem>();

        foreach (var itemDto in dto.OrderItems)
        {
            var product = await productRepository.GetByIdAsync(itemDto.ProductId)
                ?? throw new KeyNotFoundException($"Product with id {itemDto.ProductId} not found.");

            if (product.Stock < itemDto.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for product '{product.Name}'. Available: {product.Stock}, requested: {itemDto.Quantity}.");

            product.Stock -= itemDto.Quantity;
            await productRepository.UpdateAsync(product);

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price
            });
        }

        var order = new Order
        {
            CustomerId = customer.Id,
            OrderItems = orderItems,
            Total = orderItems.Sum(oi => oi.Quantity * oi.UnitPrice)
        };

        var created = await orderRepository.CreateAsync(order);
        var full = await orderRepository.GetByIdAsync(created.Id);
        return mapper.Map<OrderDto>(full!);
    }
}
