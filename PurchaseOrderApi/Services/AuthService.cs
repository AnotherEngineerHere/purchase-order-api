using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PurchaseOrderApi.DTOs.Auth;
using PurchaseOrderApi.Services.Interfaces;

namespace PurchaseOrderApi.Services;

public class AuthService(IConfiguration configuration) : IAuthService
{
    // Demo credentials — in production replace with a real user store
    private const string DemoUsername = "admin";
    private const string DemoPassword = "admin123";

    public TokenDto? Login(LoginDto dto)
    {
        if (dto.Username != DemoUsername || dto.Password != DemoPassword)
            return null;

        var jwtSettings = configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddHours(double.Parse(jwtSettings["ExpiresInHours"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, dto.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiry,
            signingCredentials: credentials);

        return new TokenDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiry
        };
    }
}
