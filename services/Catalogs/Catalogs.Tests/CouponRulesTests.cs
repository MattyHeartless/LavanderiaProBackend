using Catalogs.Domain.Entities;

namespace Catalogs.Tests;

public class CouponRulesTests
{
    [Fact]
    public void ValidateRequest_ReturnsError_WhenEventTypeIsInvalid()
    {
        var now = DateTime.UtcNow;

        var result = CouponRules.ValidateRequest(
            code: "PROMO10",
            name: "Promo 10",
            benefitType: "percentage",
            benefitValue: 10,
            eventType: "invalid_event",
            expiresAt: now.AddDays(1),
            usageLimit: 100,
            nowUtc: now);

        Assert.NotNull(result);
        Assert.Contains("Unsupported event type", result);
    }

    [Fact]
    public void ValidateRequest_ReturnsNull_WhenPayloadIsValid()
    {
        var now = DateTime.UtcNow;

        var result = CouponRules.ValidateRequest(
            code: "PROMO10",
            name: "Promo 10",
            benefitType: "percentage",
            benefitValue: 10,
            eventType: "first_order",
            expiresAt: now.AddDays(1),
            usageLimit: 100,
            nowUtc: now);

        Assert.Null(result);
    }
}
