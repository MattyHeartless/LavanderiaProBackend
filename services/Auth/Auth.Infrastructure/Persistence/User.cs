using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.Persistence;

public class User : IdentityUser
{
    public string? FullName { get; set; }
    public string AuthenticationProvider { get; set; } = AuthenticationProviders.Password;
}

public static class AuthenticationProviders
{
    public const string Password = "Password";
    public const string Google = "Google";
}
