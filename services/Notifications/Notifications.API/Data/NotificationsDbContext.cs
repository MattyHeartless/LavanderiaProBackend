using Microsoft.EntityFrameworkCore;
using Notifications.API.Models;

namespace Notifications.API.Data;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : DbContext(options)
{
    public DbSet<CourierPushSubscription> CourierPushSubscriptions => Set<CourierPushSubscription>();
    public DbSet<SmsNotificationOutbox> SmsNotificationOutbox => Set<SmsNotificationOutbox>();

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

        var sms = modelBuilder.Entity<SmsNotificationOutbox>();
        sms.HasKey(x => x.Id);
        sms.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
        sms.Property(x => x.Message).HasMaxLength(500).IsRequired();
        sms.Property(x => x.LastError).HasMaxLength(2000);
        sms.HasIndex(x => new { x.OrderId, x.CourierId }).IsUnique();
        sms.HasIndex(x => new { x.SentAt, x.NextAttemptAt, x.CreatedAt });
    }
}
