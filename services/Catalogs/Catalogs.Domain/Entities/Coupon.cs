namespace Catalogs.Domain.Entities
{
    public class Coupon
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string BenefitType { get; set; } = string.Empty;
        public decimal BenefitValue { get; set; }
        public string EventType { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public static readonly string[] SupportedEventTypes =
        {
            "first_order",
            "birthday",
            "referral",
            "reactivation",
            "seasonal_campaign"
        };
    }
}