using Microsoft.EntityFrameworkCore;
using Notifications.API.Models;

namespace Notifications.API.Data;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<CourierPushSubscription> CourierPushSubscriptions => Set<CourierPushSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var subscription = modelBuilder.Entity<CourierPushSubscription>();
        subscription.HasKey(x => x.Id);
        subscription.Property(x => x.AuthUserId).HasMaxLength(450).IsRequired();
        subscription.Property(x => x.Endpoint).HasMaxLength(2048).IsRequired();
        subscription.Property(x => x.P256dh).HasMaxLength(512).IsRequired();
        subscription.Property(x => x.Auth).HasMaxLength(512).IsRequired();
        subscription.HasIndex(x => x.Endpoint).IsUnique();
        subscription.HasIndex(x => new { x.AuthUserId, x.IsAvailable, x.IsEnabled });
    }
}
