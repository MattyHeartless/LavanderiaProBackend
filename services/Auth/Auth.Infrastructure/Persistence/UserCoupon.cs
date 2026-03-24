namespace Auth.Infrastructure.Persistence;

public class UserCoupon
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid CouponId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? RedeemedAt { get; set; }
    public string? OrderId { get; set; }
    public string? Source { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public string CouponCodeSnapshot { get; set; } = string.Empty;
    public string? CouponNameSnapshot { get; set; }
    public string? CouponDescriptionSnapshot { get; set; }
    public string BenefitTypeSnapshot { get; set; } = string.Empty;
    public decimal BenefitValueSnapshot { get; set; }
    public string EventTypeSnapshot { get; set; } = string.Empty;

    public User? User { get; set; }
}