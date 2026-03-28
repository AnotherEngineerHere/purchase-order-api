using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApi.DTOs.Auth;
using PurchaseOrderApi.Services.Interfaces;

namespace PurchaseOrderApi.Controllers;

/// <summary>Authentication endpoints.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>Login and obtain a JWT token.</summary>
    /// <remarks>Demo credentials: username = <b>admin</b>, password = <b>admin123</b></remarks>
    /// <param name="dto">Login credentials.</param>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        var result = authService.Login(dto);
        return result is null ? Unauthorized(new { error = "Invalid credentials." }) : Ok(result);
    }
}
