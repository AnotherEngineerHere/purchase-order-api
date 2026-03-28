using PurchaseOrderApi.DTOs.Auth;

namespace PurchaseOrderApi.Services.Interfaces;

public interface IAuthService
{
    TokenDto? Login(LoginDto dto);
}
