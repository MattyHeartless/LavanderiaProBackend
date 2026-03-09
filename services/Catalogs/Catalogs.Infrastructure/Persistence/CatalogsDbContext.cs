
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
        }
    }
}
