
using Microsoft.AspNetCore.Identity;
using Auth.Application.Interfaces;
using Auth.Application.DTOs;
using Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace Auth.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AuthDbContext _dbContext;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole> roleManager,
        AuthDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _dbContext = dbContext;
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
            FullName = request.FullName
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
   

    // 4. Devolver el DTO con la info que Angular necesita
    return new LoginResponse
    {
        Email = user.Email,
        FullName = user.FullName,
        id = user.Id,
        PhoneNumber = user.PhoneNumber,
    };
    }

    public async Task<LoginResponse> LoginCourierAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

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
        };
    }

     public async Task<LoginResponse> LoginAdminAsync(LoginRequest request)
    {
       // 1. Buscamos al usuario por email para poder acceder a sus datos después
    var user = await _userManager.FindByEmailAsync(request.Email);
    
    if (user == null)
        throw new UnauthorizedAccessException("Invalid credentials");

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

    public async Task ChangePasswordAsync(ChangePasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
    
        if (user == null)
            throw new KeyNotFoundException("User not found");

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
            PhoneNumber = user.PhoneNumber
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
            FullName = request.FullName
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

