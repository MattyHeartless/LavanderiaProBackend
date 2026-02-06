using Microsoft.EntityFrameworkCore;
using Profile.Domain.Entities;

namespace Profile.Infrastructure.Persistence;

public class ProfileDbContext : DbContext
{
    public ProfileDbContext(DbContextOptions<ProfileDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<UserPaymentMethod> UserPaymentMethods => Set<UserPaymentMethod>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAddress>()
            .HasIndex(x => x.UserId);

        modelBuilder.Entity<UserPaymentMethod>()
            .HasIndex(x => x.UserId);
    }
}
