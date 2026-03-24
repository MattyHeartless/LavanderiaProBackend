
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using Auth.Application.Interfaces;
using Auth.Application.DTOs;


namespace LavanderiaProBackend.Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        
          [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        RegisterResponse response;
        try
        {
            response = await _authService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("register-courier")]
    public async Task<IActionResult> RegisterCourier([FromBody] RegisterRequest request)
    {
        RegisterResponse response;
        try
        {
            response = await _authService.RegisterCourierAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("registrousuariopromo")]
    public async Task<IActionResult> RegistroUsuarioPromo([FromBody] RegistroUsuarioPromoRequest request)
    {
        RegisterResponse response;
        try
        {
            response = await _authService.RegistroUsuarioPromoAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("courier-account-exists")]
    public async Task<IActionResult> CourierAccountExists([FromQuery] string email)
    {
        var exists = await _authService.CourierAccountExistsAsync(email);
        return Ok(new { exists });
    }

        [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        LoginResponse response;
        try
        {
            response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("login-courier")]
    public async Task<IActionResult> LoginCourier([FromBody] LoginRequest request)
    {
        LoginResponse response;
        try
        {
            response = await _authService.LoginCourierAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
    
        [HttpPost("login-admin")]
    public async Task<IActionResult> LoginAdmin([FromBody] LoginRequest request)
    {
        LoginResponse response;
        try
        {
            response = await _authService.LoginAdminAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _authService.GetAllUsers();
        return Ok(users);
    }

    [HttpGet("users/{userId}/coupons")]
    public async Task<IActionResult> GetUserCoupons(string userId)
    {
        try
        {
            var coupons = await _authService.GetUserCouponsAsync(userId);
            return Ok(coupons);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


     [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            await _authService.ChangePasswordAsync(request);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    
    [HttpPut("update-user/{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
    {
        UpdateUserResponse response;
        try
        {
            response = await _authService.UpdateUserAsync(userId, request);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("validate-coupon")]
    public async Task<IActionResult> ValidateCoupon([FromBody] ValidateUserCouponRequest request)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var response = await _authService.ValidateUserCouponAsync(request, authenticatedUserId);
        if (!response.IsValid)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("redeem-coupon")]
    public async Task<IActionResult> RedeemCoupon([FromBody] RedeemUserCouponRequest request)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        try
        {
            var response = await _authService.RedeemUserCouponAsync(request, authenticatedUserId);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
}