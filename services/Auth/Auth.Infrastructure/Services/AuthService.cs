
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Auth.Application.Interfaces;
using Auth.Application.DTOs;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Auth.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AuthDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole> roleManager,
        AuthDbContext dbContext,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var user = await CreateUserAsync(request);
        return new RegisterResponse
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName
    };
    }

    public async Task<RegisterResponse> RegistroUsuarioPromoAsync(RegistroUsuarioPromoRequest request)
    {
        if (request.User == null)
            throw new InvalidOperationException("User payload is required");

        if (request.Coupon == null)
            throw new InvalidOperationException("Coupon payload is required");

        var validationError = PromoCouponRules.ValidatePayload(request.Coupon, DateTime.UtcNow);
        if (validationError != null)
            throw new InvalidOperationException(validationError);

        var user = await CreateUserAsync(request.User);

        try
        {
            if (request.Coupon.EventType.Equals(CouponEventTypes.FirstOrder, StringComparison.OrdinalIgnoreCase))
            {
                var hasActiveFirstOrderCoupon = await _dbContext.UserCoupons.AnyAsync(x =>
                    x.UserId == user.Id &&
                    x.EventTypeSnapshot == CouponEventTypes.FirstOrder &&
                    x.Status == UserCouponStatuses.Created);

                if (hasActiveFirstOrderCoupon)
                    throw new InvalidOperationException("User already has an active first_order coupon");
            }

            var now = DateTime.UtcNow;
            var assignment = new UserCoupon
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                CouponId = request.Coupon.CouponId,
                Status = UserCouponStatuses.Created,
                CreatedAt = now,
                Source = request.Source?.Trim(),
                ExpiresAt = request.Coupon.ExpiresAt,
                CouponCodeSnapshot = request.Coupon.CouponCode.Trim(),
                CouponNameSnapshot = request.Coupon.CouponName?.Trim(),
                CouponDescriptionSnapshot = request.Coupon.CouponDescription?.Trim(),
                BenefitTypeSnapshot = request.Coupon.BenefitType.Trim(),
                BenefitValueSnapshot = request.Coupon.BenefitValue,
                EventTypeSnapshot = request.Coupon.EventType.Trim()
            };

            _dbContext.UserCoupons.Add(assignment);
            await _dbContext.SaveChangesAsync();
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }

        return new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty
        };
    }

    public async Task<RegisterResponse> RegisterCourierAsync(RegisterRequest request)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            AuthenticationProvider = AuthenticationProviders.Password
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        const string courierRole = "Courier";
        var roleExists = await _roleManager.RoleExistsAsync(courierRole);

        if (!roleExists)
        {
            var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(courierRole));
            if (!createRoleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                var errors = string.Join(", ", createRoleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Courier role creation failed: {errors}");
            }
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(user, courierRole);
        if (!addToRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            var errors = string.Join(", ", addToRoleResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Courier registration failed: {errors}");
        }

        return new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName
        };
    }

    public async Task<bool> CourierAccountExistsAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        return await _userManager.IsInRoleAsync(user, "Courier");
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
       // 1. Buscamos al usuario por email para poder acceder a sus datos después
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        if (user.AuthenticationProvider == AuthenticationProviders.Google)
            throw new UnauthorizedAccessException("This account must be accessed with Google");

    // 2. Verificamos la contraseña
    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

    if (!result.Succeeded)
    {
        // Si el login falla por bloqueo de cuenta (Lockout)
        if (result.IsLockedOut)
            throw new UnauthorizedAccessException("Account is locked");
            
        throw new UnauthorizedAccessException("Invalid credentials");
    }

    var roles = await _userManager.GetRolesAsync(user);
    return CreateLoginResponse(user, roles);
    }

    public async Task<LoginResponse> LoginWithGoogleAsync(GoogleLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Credential))
            throw new UnauthorizedAccessException("Google credential is required");

        var clientId = _configuration["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("Google sign-in is not configured");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(
                request.Credential,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { clientId }
                });
        }
        catch (Exception)
        {
            throw new UnauthorizedAccessException("Invalid Google credential");
        }

        if (string.IsNullOrWhiteSpace(payload.Subject) ||
            string.IsNullOrWhiteSpace(payload.Email) ||
            payload.EmailVerified != true)
        {
            throw new UnauthorizedAccessException("Google account email could not be verified");
        }

        const string provider = AuthenticationProviders.Google;
        var user = await _userManager.FindByLoginAsync(provider, payload.Subject);

        if (user is not null && user.AuthenticationProvider != AuthenticationProviders.Google)
            throw new UnauthorizedAccessException("This account must be accessed with its password");

        if (user is null)
        {
            var existingUser = await _userManager.FindByEmailAsync(payload.Email);
            if (existingUser is not null)
            {
                throw new InvalidOperationException(
                    "An account with this email already exists. Sign in with your password to continue.");
            }

            user = new User
            {
                UserName = payload.Email,
                Email = payload.Email,
                EmailConfirmed = true,
                FullName = string.IsNullOrWhiteSpace(payload.Name) ? payload.Email : payload.Name,
                AuthenticationProvider = AuthenticationProviders.Google
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Google registration failed: {errors}");
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, new UserLoginInfo(provider, payload.Subject, provider));
            if (!addLoginResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                var errors = string.Join(", ", addLoginResult.Errors.Select(error => error.Description));
                throw new InvalidOperationException($"Could not link Google sign-in: {errors}");
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        return CreateLoginResponse(user, roles);
    }

    public async Task<LoginResponse> LoginCourierAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        if (user.AuthenticationProvider == AuthenticationProviders.Google)
            throw new UnauthorizedAccessException("This account must be accessed with Google");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                throw new UnauthorizedAccessException("Account is locked");

            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var roles = await _userManager.GetRolesAsync(user);

        if (!roles.Contains("Courier"))
            throw new UnauthorizedAccessException("User is not a courier");

        return new LoginResponse
        {
            Email = user.Email,
            FullName = user.FullName,
            id = user.Id,
            PhoneNumber = user.PhoneNumber,
            AuthenticationProvider = user.AuthenticationProvider,
            AccessToken = CreateAccessToken(user, roles),
        };
    }

     public async Task<LoginResponse> LoginAdminAsync(LoginRequest request)
    {
       // 1. Buscamos al usuario por email para poder acceder a sus datos después
    var user = await _userManager.FindByEmailAsync(request.Email);
    
    if (user == null)
        throw new UnauthorizedAccessException("Invalid credentials");

    if (user.AuthenticationProvider == AuthenticationProviders.Google)
        throw new UnauthorizedAccessException("This account must be accessed with Google");

    // 2. Verificamos la contraseña
    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

    if (!result.Succeeded)
    {
        // Si el login falla por bloqueo de cuenta (Lockout)
        if (result.IsLockedOut)
            throw new UnauthorizedAccessException("Account is locked");
            
        throw new UnauthorizedAccessException("Invalid credentials");
    }

    // 3. Generar el Token (Aquí iría tu lógica de JWT)
// Check if user is admin
var roles = await _userManager.GetRolesAsync(user);

if (!roles.Contains("Admin"))
    throw new UnauthorizedAccessException("User is not an administrator");

    // 4. Devolver el DTO con la info que Angular necesita
    return new LoginResponse
    {
        Email = user.Email,
        FullName = user.FullName,
        id = user.Id,
        PhoneNumber = user.PhoneNumber,
        AuthenticationProvider = user.AuthenticationProvider,
        AccessToken = CreateAccessToken(user, roles),
    };
    }

    public async Task<List<UserSummaryResponse>> GetAllUsers()
    {
        var allUsers = _userManager.Users.ToList();
        var nonAdminUsers = new List<UserSummaryResponse>();

        foreach (var user in allUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Admin"))
            {
                nonAdminUsers.Add(new UserSummaryResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber
                });
            }
        }

        return nonAdminUsers;
    }

    public async Task<List<UserCouponSummaryResponse>> GetUserCouponsAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new InvalidOperationException("User id is required");

        return await _dbContext.UserCoupons
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => MapCouponSummary(x))
            .ToListAsync();
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            throw new InvalidOperationException("Current password is required");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new InvalidOperationException("New password is required");

        var user = await _userManager.FindByIdAsync(userId);
    
        if (user == null)
            throw new KeyNotFoundException("User not found");

        if (user.AuthenticationProvider == AuthenticationProviders.Google)
            throw new InvalidOperationException("Google accounts do not have a password");

        var result = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
            throw new InvalidOperationException("Password change failed");
    }
    public async Task<UpdateUserResponse> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user == null)
            throw new KeyNotFoundException("User not found");

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.Email = request.Email;
        user.UserName = request.Email;

        var result = await _userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Update failed: {errors}");
        }

        return  new UpdateUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AuthenticationProvider = user.AuthenticationProvider
        };

    }

    public async Task<UpdateUserResponse> GetCurrentUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            throw new KeyNotFoundException("User not found");

        return new UpdateUserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FullName = user.FullName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            AuthenticationProvider = user.AuthenticationProvider
        };
    }

    public async Task<ValidateUserCouponResponse> ValidateUserCouponAsync(ValidateUserCouponRequest request, string? authenticatedUserId)
    {
        var resolvedUserId = ResolveUserIdOrThrow(authenticatedUserId, request.UserId);

        if (string.IsNullOrWhiteSpace(request.CouponCode))
            return new ValidateUserCouponResponse { IsValid = false, Message = "Coupon code is required" };

        if (!PromoCouponRules.IsSupportedEventType(request.EventType))
            return new ValidateUserCouponResponse { IsValid = false, Message = "Invalid event type" };

        var userCoupon = await _dbContext.UserCoupons.FirstOrDefaultAsync(x =>
            x.UserId == resolvedUserId &&
            x.CouponCodeSnapshot == request.CouponCode.Trim());

        if (userCoupon == null)
            return new ValidateUserCouponResponse { IsValid = false, Message = "Coupon does not belong to user" };

        if (!userCoupon.EventTypeSnapshot.Equals(request.EventType, StringComparison.OrdinalIgnoreCase))
            return new ValidateUserCouponResponse { IsValid = false, Message = "Coupon is not valid for this event" };

        if (!userCoupon.Status.Equals(UserCouponStatuses.Created, StringComparison.OrdinalIgnoreCase))
            return new ValidateUserCouponResponse { IsValid = false, Message = "Coupon has already been redeemed" };

        if (userCoupon.ExpiresAt.HasValue && userCoupon.ExpiresAt.Value <= DateTime.UtcNow)
            return new ValidateUserCouponResponse { IsValid = false, Message = "Coupon expired" };

        return new ValidateUserCouponResponse
        {
            IsValid = true,
            Message = "Coupon is valid",
            Coupon = MapCouponSummary(userCoupon)
        };
    }

    public async Task<UserCouponSummaryResponse> RedeemUserCouponAsync(RedeemUserCouponRequest request, string? authenticatedUserId)
    {
        var resolvedUserId = ResolveUserIdOrThrow(authenticatedUserId, request.UserId);

        if (string.IsNullOrWhiteSpace(request.CouponCode))
            throw new InvalidOperationException("Coupon code is required");

        if (string.IsNullOrWhiteSpace(request.OrderId))
            throw new InvalidOperationException("Order id is required");

        if (!PromoCouponRules.IsSupportedEventType(request.EventType))
            throw new InvalidOperationException("Invalid event type");

        var userCoupon = await _dbContext.UserCoupons.FirstOrDefaultAsync(x =>
            x.UserId == resolvedUserId &&
            x.CouponCodeSnapshot == request.CouponCode.Trim());

        if (userCoupon == null)
            throw new KeyNotFoundException("Coupon does not belong to user");

        if (!userCoupon.EventTypeSnapshot.Equals(request.EventType, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Coupon is not valid for this event");

        if (!userCoupon.Status.Equals(UserCouponStatuses.Created, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Coupon has already been redeemed");

        if (userCoupon.ExpiresAt.HasValue && userCoupon.ExpiresAt.Value <= DateTime.UtcNow)
            throw new InvalidOperationException("Coupon expired");

        userCoupon.Status = UserCouponStatuses.Redeemed;
        userCoupon.RedeemedAt = DateTime.UtcNow;
        userCoupon.OrderId = request.OrderId.Trim();

        await _dbContext.SaveChangesAsync();

        return MapCouponSummary(userCoupon);
    }

    private string CreateAccessToken(User user, IEnumerable<string> roles)
    {
        var key = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is required.");
        var expiresInMinutes = int.TryParse(_configuration["Jwt:ExpiresInMinutes"], out var configuredExpiration)
            ? configuredExpiration
            : 480;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName ?? user.Email ?? user.Id)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        return new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials));
    }

    private LoginResponse CreateLoginResponse(User user, IEnumerable<string> roles) => new()
    {
        Email = user.Email ?? string.Empty,
        FullName = user.FullName ?? string.Empty,
        id = user.Id,
        PhoneNumber = user.PhoneNumber ?? string.Empty,
        AuthenticationProvider = user.AuthenticationProvider,
        AccessToken = CreateAccessToken(user, roles)
    };

    private async Task<User> CreateUserAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new InvalidOperationException("Email is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new InvalidOperationException("Password is required");

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            AuthenticationProvider = AuthenticationProviders.Password
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        return user;
    }

    private static string ResolveUserIdOrThrow(string? authenticatedUserId, string? requestUserId)
    {
        if (!string.IsNullOrWhiteSpace(authenticatedUserId))
            return authenticatedUserId;

        if (!string.IsNullOrWhiteSpace(requestUserId))
            return requestUserId;

        throw new InvalidOperationException("User id is required");
    }

    private static UserCouponSummaryResponse MapCouponSummary(UserCoupon userCoupon)
    {
        return new UserCouponSummaryResponse
        {
            Id = userCoupon.Id,
            CouponId = userCoupon.CouponId,
            Status = userCoupon.Status,
            CreatedAt = userCoupon.CreatedAt,
            RedeemedAt = userCoupon.RedeemedAt,
            OrderId = userCoupon.OrderId,
            Source = userCoupon.Source,
            ExpiresAt = userCoupon.ExpiresAt,
            CouponCodeSnapshot = userCoupon.CouponCodeSnapshot,
            CouponNameSnapshot = userCoupon.CouponNameSnapshot,
            CouponDescriptionSnapshot = userCoupon.CouponDescriptionSnapshot,
            BenefitTypeSnapshot = userCoupon.BenefitTypeSnapshot,
            BenefitValueSnapshot = userCoupon.BenefitValueSnapshot,
            EventTypeSnapshot = userCoupon.EventTypeSnapshot
        };
    }
 
}
