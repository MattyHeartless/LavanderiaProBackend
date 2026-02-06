
using Microsoft.AspNetCore.Identity;
using Auth.Application.Interfaces;
using Auth.Application.DTOs;
using Auth.Infrastructure.Persistence;


namespace Auth.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
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
        return new RegisterResponse
    {
        Id = user.Id,
        Email = user.Email,
        FullName = user.FullName
    };
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
 
}

