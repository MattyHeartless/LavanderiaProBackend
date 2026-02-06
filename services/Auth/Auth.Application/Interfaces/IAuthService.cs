
using Auth.Application.DTOs;

namespace Auth.Application.Interfaces;
public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task ChangePasswordAsync(ChangePasswordRequest request);
    Task<UpdateUserResponse> UpdateUserAsync(string userId, UpdateUserRequest request);
}