namespace Catalogs.API.DTOs;

public class CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BenefitType { get; set; } = string.Empty;
    public decimal BenefitValue { get; set; }
    public string EventType { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public int? UsageLimit { get; set; }
}

public class UpdateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BenefitType { get; set; } = string.Empty;
    public decimal BenefitValue { get; set; }
    public string EventType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int? UsageLimit { get; set; }
}