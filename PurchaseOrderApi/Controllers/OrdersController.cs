using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApi.DTOs.Orders;
using PurchaseOrderApi.Services.Interfaces;

namespace PurchaseOrderApi.Controllers;

/// <summary>Manages purchase orders.</summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController(IOrderService service) : ControllerBase
{
    /// <summary>Get all orders.</summary>
    /// <returns>List of orders.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    /// <summary>Get an order by ID.</summary>
    /// <param name="id">Order ID.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await service.GetByIdAsync(id);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>Create a new order. Validates stock and reduces it automatically.</summary>
    /// <param name="dto">Order data with customer and items.</param>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
