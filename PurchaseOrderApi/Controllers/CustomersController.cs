using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApi.DTOs.Customers;
using PurchaseOrderApi.Services.Interfaces;

namespace PurchaseOrderApi.Controllers;

/// <summary>Manages customers.</summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CustomersController(ICustomerService service) : ControllerBase
{
    /// <summary>Get all customers.</summary>
    /// <returns>List of customers.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await service.GetAllAsync());

    /// <summary>Get a customer by ID.</summary>
    /// <param name="id">Customer ID.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await service.GetByIdAsync(id);
        return customer is null ? NotFound() : Ok(customer);
    }

    /// <summary>Create a new customer.</summary>
    /// <param name="dto">Customer data.</param>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
