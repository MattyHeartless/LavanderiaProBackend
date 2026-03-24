
using Catalogs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalogs.Infrastructure.Persistence
{
    public class CatalogsDbContext : DbContext
    {
        public CatalogsDbContext(DbContextOptions<CatalogsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }
        public DbSet<Courier> Couriers { get; set; }
        public DbSet<Coupon> Coupons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
                    base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Service>(builder =>
                {
                    builder.HasKey(x => x.Id);

                    builder.Property(x => x.Id)
                        .ValueGeneratedOnAdd();

                    builder.Property(x => x.Name)
                        .IsRequired()
                        .HasMaxLength(100);

                    builder.Property(x => x.Price)
                        .HasPrecision(18, 2);
                });

                modelBuilder.Entity<Courier>(builder =>
                {
                    builder.HasKey(x => x.Id);

                    builder.Property(x => x.Id)
                        .ValueGeneratedOnAdd();

                    builder.Property(x => x.Name)
                        .IsRequired()
                        .HasMaxLength(100);

                    builder.Property(x => x.ProfileImageUrl)
                        .HasMaxLength(500);
                });

                modelBuilder.Entity<Coupon>(builder =>
                {
                    builder.HasKey(x => x.Id);

                    builder.Property(x => x.Id)
                        .ValueGeneratedOnAdd();

                    builder.Property(x => x.Code)
                        .IsRequired()
                        .HasMaxLength(50);

                    builder.HasIndex(x => x.Code)
                        .IsUnique();

                    builder.Property(x => x.Name)
                        .IsRequired()
                        .HasMaxLength(150);

                    builder.Property(x => x.Description)
                        .HasMaxLength(500);

                    builder.Property(x => x.BenefitType)
                        .IsRequired()
                        .HasMaxLength(50);

                    builder.Property(x => x.BenefitValue)
                        .HasPrecision(18, 2);

                    builder.Property(x => x.EventType)
                        .IsRequired()
                        .HasMaxLength(50);

                    builder.Property(x => x.CreatedAt)
                        .IsRequired();

                    builder.Property(x => x.UpdatedAt)
                        .IsRequired();
                });
        }
    }
}
