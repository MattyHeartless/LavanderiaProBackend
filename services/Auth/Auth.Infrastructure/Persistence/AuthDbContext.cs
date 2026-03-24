using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Auth.Application.DTOs;

namespace Auth.Infrastructure.Persistence;

public class AuthDbContext 
    : IdentityDbContext<User, IdentityRole, string>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserCoupon> UserCoupons => Set<UserCoupon>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserCoupon>(entity =>
        {
            entity.ToTable("UserCoupons");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(x => x.Source)
                .HasMaxLength(100);

            entity.Property(x => x.OrderId)
                .HasMaxLength(100);

            entity.Property(x => x.CouponCodeSnapshot)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.CouponNameSnapshot)
                .HasMaxLength(150);

            entity.Property(x => x.CouponDescriptionSnapshot)
                .HasMaxLength(500);

            entity.Property(x => x.BenefitTypeSnapshot)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.BenefitValueSnapshot)
                .HasPrecision(18, 2);

            entity.Property(x => x.EventTypeSnapshot)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(x => new { x.UserId, x.CouponCodeSnapshot })
                .IsUnique();

            entity.HasIndex(x => new { x.UserId, x.EventTypeSnapshot, x.Status })
                .IsUnique()
                .HasFilter($"[Status] = '{UserCouponStatuses.Created}'");

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    
}