namespace Catalogs.Domain.Entities;

public static class CouponRules
{
    public static string? ValidateRequest(
        string? code,
        string? name,
        string? benefitType,
        decimal benefitValue,
        string? eventType,
        DateTime? expiresAt,
        int? usageLimit,
        DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(code))
            return "Code is required";

        if (string.IsNullOrWhiteSpace(name))
            return "Name is required";

        if (string.IsNullOrWhiteSpace(benefitType))
            return "Benefit type is required";

        if (benefitValue <= 0)
            return "Benefit value must be greater than 0";

        if (string.IsNullOrWhiteSpace(eventType))
            return "Event type is required";

        var normalizedEvent = eventType.Trim();
        if (!Coupon.SupportedEventTypes.Contains(normalizedEvent, StringComparer.OrdinalIgnoreCase))
            return $"Unsupported event type. Allowed values: {string.Join(", ", Coupon.SupportedEventTypes)}";

        if (expiresAt.HasValue && expiresAt.Value <= nowUtc)
            return "Expiration date must be in the future";

        if (usageLimit.HasValue && usageLimit.Value <= 0)
            return "Usage limit must be greater than 0";

        return null;
    }
}