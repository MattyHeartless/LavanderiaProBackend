using Auth.Application.DTOs;

namespace Auth.Tests;

public class PromoCouponRulesTests
{
    [Fact]
    public void ValidatePayload_ReturnsError_WhenCouponIdIsEmpty()
    {
        var payload = new PromoCouponPayload
        {
            CouponId = Guid.Empty,
            CouponCode = "PROMO10",
            BenefitType = "percentage",
            BenefitValue = 10,
            EventType = CouponEventTypes.FirstOrder,
            ExpiresAt = DateTime.UtcNow.AddDays(2)
        };

        var result = PromoCouponRules.ValidatePayload(payload, DateTime.UtcNow);

        Assert.Equal("Coupon id is required", result);
    }

    [Fact]
    public void IsSupportedEventType_ReturnsTrue_ForConfiguredEvents()
    {
        var result = PromoCouponRules.IsSupportedEventType(CouponEventTypes.Referral);

        Assert.True(result);
    }
}
