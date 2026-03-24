
using Auth.Application.DTOs;


namespace Auth.Application.Interfaces;
public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<RegisterResponse> RegistroUsuarioPromoAsync(RegistroUsuarioPromoRequest request);
    Task<RegisterResponse> RegisterCourierAsync(RegisterRequest request);
    Task<bool> CourierAccountExistsAsync(string email);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> LoginCourierAsync(LoginRequest request);
    Task<LoginResponse> LoginAdminAsync(LoginRequest request);
    Task<List<UserSummaryResponse>> GetAllUsers();
    Task<List<UserCouponSummaryResponse>> GetUserCouponsAsync(string userId);
    Task ChangePasswordAsync(ChangePasswordRequest request);
    Task<UpdateUserResponse> UpdateUserAsync(string userId, UpdateUserRequest request);
    Task<ValidateUserCouponResponse> ValidateUserCouponAsync(ValidateUserCouponRequest request, string? authenticatedUserId);
    Task<UserCouponSummaryResponse> RedeemUserCouponAsync(RedeemUserCouponRequest request, string? authenticatedUserId);

   
    

}