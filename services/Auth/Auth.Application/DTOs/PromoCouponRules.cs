namespace Auth.Application.DTOs;

public static class PromoCouponRules
{
    public static bool IsSupportedEventType(string? eventType)
    {
        return !string.IsNullOrWhiteSpace(eventType)
            && CouponEventTypes.Supported.Contains(eventType, StringComparer.OrdinalIgnoreCase);
    }

    public static string? ValidatePayload(PromoCouponPayload payload, DateTime nowUtc)
    {
        if (payload.CouponId == Guid.Empty)
            return "Coupon id is required";

        if (string.IsNullOrWhiteSpace(payload.CouponCode))
            return "Coupon code is required";

        if (string.IsNullOrWhiteSpace(payload.BenefitType))
            return "Benefit type is required";

        if (payload.BenefitValue <= 0)
            return "Benefit value must be greater than 0";

        if (!IsSupportedEventType(payload.EventType))
            return "Invalid event type";

        if (payload.ExpiresAt.HasValue && payload.ExpiresAt.Value <= nowUtc)
            return "Coupon expiration must be in the future";

        return null;
    }
}