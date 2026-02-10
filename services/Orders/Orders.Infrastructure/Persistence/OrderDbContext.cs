using Microsoft.EntityFrameworkCore;
using Orders.Domain.Entities;

namespace Orders.Infrastructure.Persistence;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Order>(builder =>
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(o => o.UserAddressId)
            .IsRequired();

        builder.Property(o => o.UserPaymentMethodId);

        builder.Property(o => o.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.DeliveryFee)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.PostPaymentMethod)
            .HasMaxLength(50);

        // ---------- ShippingAddress (Value Object)
        builder.OwnsOne(o => o.ShippingAddress, sa =>
        {
            sa.Property(p => p.Title)
                .HasMaxLength(100)
                .IsRequired();

            sa.Property(p => p.Street)
                .HasMaxLength(200)
                .IsRequired();

            sa.Property(p => p.Neighbourhood)
                .HasMaxLength(100);

            sa.Property(p => p.City)
                .HasMaxLength(100)
                .IsRequired();

            sa.Property(p => p.State)
                .HasMaxLength(100)
                .IsRequired();

            sa.Property(p => p.ZipCode)
                .HasMaxLength(20)
                .IsRequired();
        });
    });
}

}
