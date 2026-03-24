namespace Auth.Application.DTOs;

public static class UserCouponStatuses
{
    public const string Created = "CREATED";
    public const string Redeemed = "REDEEMED";
}

public static class CouponEventTypes
{
    public const string FirstOrder = "first_order";
    public const string Birthday = "birthday";
    public const string Referral = "referral";
    public const string Reactivation = "reactivation";
    public const string SeasonalCampaign = "seasonal_campaign";

    public static readonly string[] Supported =
    {
        FirstOrder,
        Birthday,
        Referral,
        Reactivation,
        SeasonalCampaign
    };
}

public class PromoCouponPayload
{
    public Guid CouponId { get; set; }
    public string CouponCode { get; set; } = string.Empty;
    public string? CouponName { get; set; }
    public string? CouponDescription { get; set; }
    public string BenefitType { get; set; } = string.Empty;
    public decimal BenefitValue { get; set; }
    public string EventType { get; set; } = CouponEventTypes.FirstOrder;
    public DateTime? ExpiresAt { get; set; }
}

public class RegistroUsuarioPromoRequest
{
    public RegisterRequest User { get; set; } = new();
    public PromoCouponPayload Coupon { get; set; } = new();
    public string? Source { get; set; }
}

public class UserCouponSummaryResponse
{
    public Guid Id { get; set; }
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
}

public class ValidateUserCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
    public string EventType { get; set; } = CouponEventTypes.FirstOrder;
    public string? UserId { get; set; }
}

public class ValidateUserCouponResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserCouponSummaryResponse? Coupon { get; set; }
}

public class RedeemUserCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string EventType { get; set; } = CouponEventTypes.FirstOrder;
    public string? UserId { get; set; }
}