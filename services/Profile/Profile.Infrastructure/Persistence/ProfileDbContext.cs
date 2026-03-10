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
        modelBuilder.Entity<UserAddress>(entity =>
        {
            entity.HasIndex(x => x.UserId);
            entity.Property(x => x.Latitude).HasPrecision(9, 6);
            entity.Property(x => x.Longitude).HasPrecision(9, 6);
        });

        modelBuilder.Entity<UserPaymentMethod>()
            .HasIndex(x => x.UserId);
    }
}
