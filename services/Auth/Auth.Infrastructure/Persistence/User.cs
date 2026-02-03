using Microsoft.AspNetCore.Identity;

namespace Auth.Infrastructure.Persistence;

public class User : IdentityUser
{
    public string? FullName { get; set; }
}