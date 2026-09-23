
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

    [HttpPost("google")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] GoogleLoginRequest request)
    {
        try
        {
            var response = await _authService.LoginWithGoogleAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
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

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [HttpGet("users/recent-customers")]
    public async Task<IActionResult> GetRecentCustomers([FromQuery] int limit = 50)
    {
        if (limit is < 1 or > 50)
            return BadRequest(new { message = "Limit must be between 1 and 50." });

        return Ok(await _authService.GetRecentCustomersAsync(limit));
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


    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            await _authService.ChangePasswordAsync(userId, request);
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
    
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpPut("update-user/{userId}")]
    public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.Equals(userId, authenticatedUserId, StringComparison.Ordinal))
            return Forbid();

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

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        try
        {
            return Ok(await _authService.GetCurrentUserAsync(userId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
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
