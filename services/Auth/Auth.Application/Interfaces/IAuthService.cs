
using Auth.Application.DTOs;


namespace Auth.Application.Interfaces;
public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<RegisterResponse> RegisterCourierAsync(RegisterRequest request);
    Task<bool> CourierAccountExistsAsync(string email);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> LoginCourierAsync(LoginRequest request);
    Task<LoginResponse> LoginAdminAsync(LoginRequest request);
    Task<List<UserSummaryResponse>> GetAllUsers();
    Task ChangePasswordAsync(ChangePasswordRequest request);
    Task<UpdateUserResponse> UpdateUserAsync(string userId, UpdateUserRequest request);

   
    

}