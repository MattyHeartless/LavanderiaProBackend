namespace Catalogs.API.DTOs;

public class CreatePricingOptionRequest
{
    public string OptionName { get; set; } = default!;
    public decimal Price { get; set; }
    public string UoM { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}

public class UpdatePricingOptionRequest
{
    public string OptionName { get; set; } = default!;
    public decimal Price { get; set; }
    public string UoM { get; set; } = default!;
    public bool IsActive { get; set; }
}
