
using Auth.Application.DTOs;

namespace Auth.Application.Interfaces;
public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request);
    Task LoginAsync(LoginRequest request);
    Task ChangePasswordAsync(ChangePasswordRequest request);
}