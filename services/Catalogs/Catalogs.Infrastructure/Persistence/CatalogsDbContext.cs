
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
                });

                modelBuilder.Entity<Courier>(builder =>
                {
                    builder.HasKey(x => x.Id);

                    builder.Property(x => x.Id)
                        .ValueGeneratedOnAdd();

                    builder.Property(x => x.Name)
                        .IsRequired()
                        .HasMaxLength(100);
                });
        }
    }
}
